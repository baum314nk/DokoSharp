using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using DokoSharp.Lib;
using DokoSharp.Lib.Messaging;
using System.IO;
using DokoTable.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using DokoTable.ViewModels.Commands;
using DokoTable.ViewModels.WindowDialogService;
using System.Text.Json;
using System.Net.Sockets;
using DokoTable.Models;

namespace DokoTable.ViewModels;

/// <summary>
/// The viewmodel that handles a Doko game.
/// </summary>
public class GameViewModel : BaseViewModel
{
    #region Fields

    private readonly MainViewModel _parent;
    private readonly IWindowDialogService _dialogService;
    private readonly DokoTcpClient _client;
    private IReadOnlyDictionary<CardBase, BitmapImage>? _cardImageSet = null;

    private string _infoText = "";
    private CardBase? _selectedHandCard;
    private Announcement _selectedAnnouncement;

    #endregion

    #region Properties

    /// <summary>
    /// A mapping from card identifiers to images.
    /// </summary>
    public IReadOnlyDictionary<CardBase, BitmapImage>? CardImageSet
    {
        get => _cardImageSet;
        set
        {
            _cardImageSet = value;
            RaisePropertyChanged(nameof(CardImageSet));
        }
    }

    /// <summary>
    /// The current state of the game.
    /// </summary>
    public GameState State { get; set; }

    /// <summary>
    /// The name of the player.
    /// </summary>
    public string PlayerName { get; private set; }

    /// <summary>
    /// The information text which is displayed above the hand cards.
    /// </summary>
    public string InformationText
    {
        get => _infoText;
        protected set
        {
            if (value == _infoText) return;

            _infoText = value;
            RaisePropertyChanged(nameof(InformationText));
        }
    }

    /// <summary>
    /// The currently selected hand card.
    /// </summary>
    public CardBase? SelectedHandCard
    {
        get => _selectedHandCard;
        set
        {
            _selectedHandCard = value;
            RaisePropertyChanged(nameof(SelectedHandCard));

            // Set in reply values of client if it exists
            if (_client == null) return;
            _client.ReplyValues["PlaceCard"] = SelectedHandCard;
            _client.ReplyValues["Announcement"] = SelectedAnnouncement;
            _client.ReplyValuesUpdated.Set();
        }
    }

    /// <summary>
    /// The currently selected announcement.
    /// </summary>
    public Announcement SelectedAnnouncement
    {
        get => _selectedAnnouncement;
        set
        {
            _selectedAnnouncement = value;
            RaisePropertyChanged(nameof(SelectedAnnouncement));
        }
    }

    #endregion

    public GameViewModel(Dispatcher dispatcher, MainViewModel parent) : base(dispatcher)
    {
        _parent = parent;
        _parent.PropertyChanged += Parent_PropertyChanged;

        PlayerName = parent.AccountName!;
        State = new();

        _dialogService = parent.DialogService!;
        _client = parent.Client!;
        _client.MessageReceived += Client_MessageReceived;
    }

    private void Parent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_parent.CardImageSet):
                CardImageSet = _parent.CardImageSet;
                break;
        }
    }

    #region Message Handlers

    private void Client_MessageReceived(object sender, DokoTcpClient.MessageEventArgs e)
    {
        Log.Debug("Message received:\n{msg}", JsonSerializer.Serialize(e.Message, Utils.BeautifyJsonOptions));

        switch (e.Message)
        {
            // Requests
            case RequestColorMessage msg:
                Client_RequestColor(msg);
                break;
            case RequestYesNoMessage msg:
                Client_RequestYesNo(msg);
                break;
            case RequestPlaceCardMessage msg:
                Log.Information("Waiting until a valid card is selected.");
                SelectedHandCard = null;
                State.CanMakeAnnouncement = msg.CanMakeAnnouncement;
                if (State.CanMakeAnnouncement) State.AvailableAnnouncements = State.GetAvailableAnnouncements();
                break;
            case RequestCardsMessage msg:
                Log.Information("Waiting until a set of {amount} cards is selected.", msg.Amount);
                Log.Error("The dialog for selecting multiple cards is currently not implemented.");
                break;
            case RequestReservationMessage msg:
                Client_RequestReservation(msg);
                break;
            // Update
            case GameCreatedMessage msg:
                Log.Information("Doko game was created by the server. Players are {players}. The special rules are {sr}.", msg.Players, msg.SpecialRules);
                break;
            case GameStartedMessage msg:
                Log.Information("Doko game has started. {nor} rounds will be played.", msg.NumberOfRounds);
                State.NumberOfRounds = msg.NumberOfRounds;
                break;
            case RoundCreatedMessage msg:
                Log.Information("Round {rn} was created. Player order is {order}.", msg.RoundNumber, msg.PlayerOrder);
                State.RoundNumber = msg.RoundNumber;
                break;
            case RoundStartedMessage msg:
                Log.Information("Round has started. Trump ranking is {tr}.", msg.TrumpRanking);
                State.TrumpCards = CardBase.GetByIdentifiers(msg.TrumpRanking!).ToList();
                State.HandCards = State.SortCardsByRanking(State.HandCards).ToList();
                break;
            case CardsReceivedMessage msg:
                Client_ReceivedCards(msg);
                break;
            case ReservationsPerformedMessage msg:
                Log.Information("Reservations have been performed. Active reservation is {tr}.", msg.ActiveReservation ?? "none");
                break;
            case RegistrationsAppliedMessage msg:
                Log.Information("Registrations have been performed. Trump ranking is {tr}.", msg.TrumpRanking);
                State.TrumpCards = CardBase.GetByIdentifiers(msg.TrumpRanking!).ToList();
                State.HandCards = State.SortCardsByRanking(State.HandCards).ToList();
                break;
            case TrickCreatedMessage msg:
                Log.Information("Trick {tn} was created. Player order is {order}.", msg.TrickNumber, msg.PlayerOrder);
                State.TrickNumber = msg.TrickNumber;
                break;
            case AnnouncementMadeMessage msg:
                Log.Information("Player {player} made the announcement {announcement}.", msg.Player, msg.Announcement.GetName());
                if (msg.Player == PlayerName) State.Announcement = msg.Announcement;
                break;
            case CardPlacedMessage msg:
                Client_CardPlaced(msg);
                break;
            case TrickFinishedMessage msg:
                Log.Information("Trick has finished. Winner of {value} value is {winner}.", msg.Value, msg.Winner);
                State.EndTrick();
                break;
            case RoundFinishedMessage msg:
                Client_RoundFinished(msg);
                break;
            case PointsUpdatedMessage msg:
                Log.Information("Player points where updated: {updates}", msg.PointChanges);
                break;
            case GameFinishedMessage msg:
                Log.Information("Doko game has finished. The finial ranking is {ranking}.", msg.Ranking);
                break;
        }

        InformationText = e.Message.Subject!;
    }

    private void Client_ReceivedCards(CardsReceivedMessage msg)
    {
        Log.Information("Received hand cards: {cards}", msg.ReceivedCards);
        var receivedCards = msg.ReceivedCards!.Select(id => CardBase.GetByIdentifier(id)!);

        List<CardBase> newCards = msg.ClearedOldCards ?
            receivedCards.ToList() :
            State.HandCards.Concat(receivedCards).ToList();

        State.HandCards = State.SortCardsByRanking(newCards).ToList();
    }

    private void Client_CardPlaced(CardPlacedMessage msg)
    {
        Log.Information("Player {player} placed {card}.", msg.Player, msg.PlacedCard);

        var card = CardBase.GetByIdentifier(msg.PlacedCard!)!;
        State.PlacedCards = State.PlacedCards.Append(card).ToList();

        // Re-evaluate placeable hand cards if first card was placed
        if (State.PlacedCards.Count == 1)
        {
            State.PlaceableHandCards = State.GetPlaceableHandCards();
        }
        // De-select card & announcement and remove card from hand if self placed it
        if (msg.Player == PlayerName)
        {
            SelectedHandCard = null;
            SelectedAnnouncement = Announcement.None;
            State.CanMakeAnnouncement = false;
            State.RemoveHandCard(card);
        }
    }

    private void Client_RequestColor(RequestColorMessage msg)
    {
        Log.Information("Waiting until a color is selected.");

        _dialogService.ShowChoiceDialog(msg.RequestText!, Enum.GetValues<CardColor>(), out CardColor color);

        _client!.ReplyValues["Color"] = color;
        _client.ReplyValuesUpdated.Set();

        Log.Information("Selected color {color}.", color);
    }

    private void Client_RequestYesNo(RequestYesNoMessage msg)
    {
        Log.Information("Waiting until yes-no decision is made.");

        _dialogService.ShowYesNoDialog(msg.RequestText!, out bool isYes);

        _client!.ReplyValues["YesNo"] = isYes;
        _client.ReplyValuesUpdated.Set();

        Log.Information("Selected {yesNo}.", isYes ? "yes" : "no");
    }

    private void Client_RequestReservation(RequestReservationMessage msg)
    {
        Log.Information("Waiting until a reservation is selected from the possibilities.");

        _dialogService.ShowChoiceDialog("Please select a reservation", msg.Possibilities!.Prepend(string.Empty), out string reservation);

        _client!.ReplyValues["Reservation"] = reservation;
        _client.ReplyValuesUpdated.Set();

        if (reservation == string.Empty) Log.Information("Selected no reservation.", reservation);
        else Log.Information("Selected reservation {reservation}.", reservation);
    }

    private void Client_RoundFinished(RoundFinishedMessage msg)
    {
        Log.Information("Round has finished. Winner of {points} is {winner}. Re party are {reParty} with {reValue} total value and points {rePoints}. Contra party are {contraParty} with {contraValue} total value and points {contraPoints}.",
            msg.BasePoints,
            ((bool)msg.RePartyWon!) ? "Re party" : "Contra party",
            msg.ReParty, msg.ReValue, msg.RePoints,
            msg.ContraParty, msg.ContraValue, msg.ContraPoints
        );

        State.EndRound();

        // Show dialog with result info
        _dialogService.ShowInfoDialog($"Results of round {State.RoundNumber}",
$@"Winner: {(msg.RePartyWon ? "Re" : "Contra")} party
Base points: {msg.BasePoints}
Re party:
  Members: {string.Join(", ", msg.ReParty!)}
  Value: {msg.ReValue}
  Points: 
{string.Join("    \n", msg.RePoints!)}
Contra party:
  Members: {string.Join(", ", msg.ContraParty!)}
  Value: {msg.ContraValue}
  Points:
{string.Join("    \n", msg.ContraPoints!)}
");
    }

    #endregion

}
