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

namespace DokoTable.ViewModels;

public class DokoViewModel : IViewModel, INotifyPropertyChanged, IDisposable
{
    #region Fields

    private readonly ImageLoader _imageLoader;
    private Dictionary<CardBase, BitmapImage> _cardImages;

    private string _serverHostname = "127.0.0.1";
    private int _serverPort = 1234;
    private DokoClient? _client;
    private List<CardBase> _trumpCards;
    private List<CardBase> _handCards;
    private List<CardBase> _placedCards;
    private List<CardBase> _placeableCards;
    private string _infoText = "";
    private int _numberOfRounds = 4;
    private int _currentRoundNumber = 0;
    private int _currentTrickNumber = 4;
    private bool disposedValue;

    // Reply values
    private CardBase? _selectedHandCard;

    #endregion

    #region Properties

    /// <summary>
    /// The dialog service used to show dialogs.
    /// </summary>
    public IWindowDialogService? DialogService { get; set; }

    /// <summary>
    /// The names of the available image sets.
    /// </summary>
    public ICollection<string> AvailableImageSets => _imageLoader.AvailableSets.ToList();

    /// <summary>
    /// A mapping from card identifiers to images.
    /// </summary>
    public IReadOnlyDictionary<CardBase, BitmapImage> CardImages
    {
        get => _cardImages;
        protected set
        {
            if (value == _cardImages) return;

            _cardImages = new(value);
            RaisePropertyChanged(nameof(CardImages));
        }
    }

    /// <summary>
    /// The hostname of the server to connect to.
    /// </summary>
    public string ServerHostname
    {
        get => _serverHostname;
        set
        {
            if (value == _serverHostname) return;

            _serverHostname = value;
            RaisePropertyChanged(nameof(ServerHostname));
        }
    }

    /// <summary>
    /// The port of the server to connect to.
    /// </summary>
    public string ServerPort
    {
        get => _serverPort.ToString();
        set
        {
            if (value == ServerPort) return;

            _serverPort = int.Parse(value);
            RaisePropertyChanged(nameof(ServerPort));
        }
    }

    /// <summary>
    /// The name of the player.
    /// </summary>
    public string PlayerName => "player1";

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
    /// The number of rounds of Doko that should be played.
    /// </summary>
    public int NumberOfRounds
    {
        get => _numberOfRounds;
        set
        {
            if (value == _numberOfRounds) return;

            _numberOfRounds = value;
            RaisePropertyChanged(nameof(NumberOfRounds));
        }
    }

    /// <summary>
    /// The number of the current Doko round.
    /// </summary>
    public int CurrentRoundNumber
    {
        get => _currentRoundNumber;
        set
        {
            if (value == _currentRoundNumber) return;

            _currentRoundNumber = value;
            RaisePropertyChanged(nameof(CurrentRoundNumber));
        }
    }

    /// <summary>
    /// The number of the current Doko trick.
    /// </summary>
    public int CurrentTrickNumber
    {
        get => _currentTrickNumber;
        set
        {
            if (value == _currentTrickNumber) return;

            _currentTrickNumber = value;
            RaisePropertyChanged(nameof(CurrentTrickNumber));
        }
    }

    public IReadOnlyList<CardBase> TrumpCards
    {
        get => _trumpCards;
        protected set
        {
            if (_trumpCards == value) return;

            _trumpCards = new(value);
            RaisePropertyChanged(nameof(TrumpCards));
            SortHandCards();
        }
    }

    /// <summary>
    /// The hand cards of the player.
    /// </summary>
    public IReadOnlyList<CardBase> HandCards
    {
        get => _handCards;
        protected set
        {
            if (_handCards == value) return;

            _handCards = new(value);
            RaisePropertyChanged(nameof(HandCards));
        }
    }

    /// <summary>
    /// The already placed cards of the trick.
    /// </summary>
    public IReadOnlyList<CardBase> PlacedCards
    {
        get => _placedCards;
        protected set
        {
            if (_placedCards == value) return;

            _placedCards = new(value);
            RaisePropertyChanged(nameof(PlacedCards));
        }
    }

    /// <summary>
    /// An enumeration of all cards that can be placed on the already placed cards.
    /// </summary>
    public IEnumerable<CardBase> PlaceableHandCards
    {
        get => _placeableCards;
        protected set
        {
            if (_placeableCards == value) return;

            _placeableCards = new(value);
            RaisePropertyChanged(nameof(PlaceableHandCards));
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
            //// Skip if value contains same elements
            //if (SelectedHandCard.Intersect(value).Count() == SelectedHandCard.Count) return;

            _selectedHandCard = value;
            RaisePropertyChanged(nameof(SelectedHandCard));

            // Set in reply values of client if it exists
            if (_client == null) return;
            //if (SelectedHandCard.Count == 1)
            //{
            _client.ReplyValues["PlaceCard"] = SelectedHandCard;
            //    _client.ReplyValues["Cards"] = null;
            //}
            //else
            //{
            //_client.ReplyValues["PlaceCard"] = null;
            //_client.ReplyValues["Cards"] = SelectedHandCard;
            //}
            _client.ReplyValuesUpdated.Set();
        }
    }

    #endregion

    #region Commands

    public ICommand LoadDefaultImageSetCommand { get; init; }
    private async Task DoLoadDefaultImageSet()
    {
        await DoDetectImageSets();
        var setName = "Deutsche Kriegsspielkarten"; //_imageLoader.AvailableSets.First();
        await DoLoadImageSet(setName);
    }

    public ICommand LoadImageSetCommand { get; init; }
    private async Task DoLoadImageSet(string setName)
    {
        var rawImages = await Task.Run(() => _imageLoader.LoadImages(setName));

        // Create BitmapImages from bytes
        // Needs to be done on UI thread
        var images = new Dictionary<CardBase, BitmapImage>();
        foreach (var kv in rawImages)
        {
            var rawImg = kv.Value;

            var img = new BitmapImage();
            using (var ms = new MemoryStream(rawImg))
            {
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
            }

            images[kv.Key] = img;
        }
        CardImages = images;

        Log.Information("Loaded image set \"{setName}\"", setName);
    }

    public ICommand DetectImageSetsCommand { get; init; }
    private async Task DoDetectImageSets()
    {
        await Task.Run(_imageLoader.DetectImageSetsAsync);

        RaisePropertyChanged(nameof(AvailableImageSets));

        Log.Information("Detected image sets: {set}", _imageLoader.AvailableSets);
    }

    public ICommand ConnectCommand { get; init; }
    private void DoConnect()
    {
        _client = new DokoClient(ServerHostname, _serverPort);
        _client.MessageReceived += (sender, e) => BeginInvoke(() => Client_MessageReceived(sender, e));

        _client.Start();
    }

    #endregion

    public DokoViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;

        _imageLoader = new();
        _cardImages = new();
        _trumpCards = new();
        _handCards = new();
        _placedCards = new();
        _placeableCards = new();

        LoadDefaultImageSetCommand = new AsyncCommand(DoLoadDefaultImageSet);
        LoadImageSetCommand = new RelayCommand<string>(
            async (setName) => await DoLoadImageSet(setName!), 
            (setName) => _imageLoader.AvailableSets.Contains(setName ?? string.Empty)
        );
        DetectImageSetsCommand = new AsyncCommand(DoDetectImageSets);
        ConnectCommand = new SimpleCommand(DoConnect, () => _client == null);
    }

    #region Message Handlers

    private void Client_MessageReceived(object sender, DokoClient.MessageEventArgs e)
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
            case RequestPlaceCardMessage:
                Log.Information("Waiting until a valid card is selected.");
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
                Log.Information("Doko game was created by the server. Players are {players}. The special rules are {sr}", msg.Players, msg.SpecialRules);
                break;
            case GameStartedMessage msg:
                Log.Information("Doko game has started. {nor} rounds will be played.", msg.NumberOfRounds);
                break;
            case RoundCreatedMessage msg:
                Log.Information("Round {rn} was created. Player order is {order}.", msg.RoundNumber, msg.PlayerOrder);
                break;
            case RoundStartedMessage msg:
                Log.Information("Round has started. Trump ranking is {tr}.", msg.TrumpRanking);
                break;
            case CardsReceivedMessage msg:
                Client_ReceivedCards(msg);
                break;
            case ReservationsPerformedMessage msg:
                Log.Information("Reservations have been performed. Active reservation is {tr}.", msg.ActiveReservation ?? "none");
                break;
            case RegistrationsAppliedMessage msg:
                Client_RegistrationsApplied(msg);
                break;
            case TrickCreatedMessage msg:
                Log.Information("Trick {tn} was created. Player order is {order}.", msg.TrickNumber, msg.PlayerOrder);
                break;
            case CardPlacedMessage msg:
                Client_CardPlaced(msg);
                break;
            case TrickFinishedMessage msg:
                PlacedCards = new List<CardBase>();
                PlaceableHandCards = new List<CardBase>();
                Log.Information("Trick has finished. Winner of {value} value is {winner}.", msg.Value, msg.Winner);
                break;
            case RoundFinishedMessage msg:
                HandCards = new List<CardBase>();
                TrumpCards = new List<CardBase>();
                Log.Information("Round has finished. Winner of {points} is {winner}. Re party are {reParty} with {reValue} total value and additional points {rePoints}. Contra party are {contraParty} with {contraValue} total value and additional points {contraPoints}.",
                    msg.BasePoints,
                    ((bool)msg.RePartyWon!) ? "Re party" : "Contra party",
                    msg.ReParty, msg.ReValue, msg.ReAdditionalPoints,
                    msg.ContraParty, msg.ContraValue, msg.ContraAdditionalPoints
                );
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

        if (msg.ClearedOldCards)
        {
            HandCards = receivedCards.ToList();
        }
        else
        {
            HandCards = HandCards.Concat(receivedCards).ToList();
        }

        SortHandCards();
    }

    private void Client_RegistrationsApplied(RegistrationsAppliedMessage msg)
    {
        Log.Information("Registrations have been performed. Trump ranking is {tr}.", msg.TrumpRanking);

        TrumpCards = CardBase.GetByIdentifiers(msg.TrumpRanking!).ToList();
    }

    private void Client_CardPlaced(CardPlacedMessage msg)
    {
        Log.Information("Player {player} placed {card}.", msg.Player, msg.PlacedCard);

        var card = CardBase.GetByIdentifier(msg.PlacedCard!)!;
        PlacedCards = PlacedCards.Append(card).ToList();

        // Re-evaluate placeable hand cards if first card was placed
        if (PlacedCards.Count == 1)
        {
            PlaceableHandCards = GetPlaceableHandCards();
        }
        // De-select card & remove it from hand if self placed it
        if (msg.Player == PlayerName)
        {
            SelectedHandCard = null;
            RemoveHandCard(card);
        }
    }

    private void Client_RequestColor(RequestColorMessage msg)
    {
        Log.Information("Waiting until a color is selected.");

        DialogService!.ShowChoiceDialog(msg.RequestText!, Enum.GetValues<CardColor>(), out CardColor color);

        _client!.ReplyValues["Color"] = color;
        _client.ReplyValuesUpdated.Set();

        Log.Information("Selected color {color}.", color);
    }

    private void Client_RequestYesNo(RequestYesNoMessage msg)
    {
        Log.Information("Waiting until yes-no decision is made.");

        DialogService!.ShowYesNoDialog(msg.RequestText!, out bool isYes);

        _client!.ReplyValues["YesNo"] = isYes;
        _client.ReplyValuesUpdated.Set();

        Log.Information("Selected {yesNo}.", isYes ? "yes" : "no");
    }

    private void Client_RequestReservation(RequestReservationMessage msg)
    {
        Log.Information("Waiting until a reservation is selected from the possibilities.");

        DialogService!.ShowChoiceDialog("Please select a reservation", msg.Possibilities!.Prepend(string.Empty), out string reservation);

        _client!.ReplyValues["Reservation"] = reservation;
        _client.ReplyValuesUpdated.Set();

        if (reservation == string.Empty) Log.Information("Selected no reservation.", reservation);
        else Log.Information("Selected reservation {reservation}.", reservation);
    }

    #endregion

    private IEnumerable<CardBase> GetPlaceableHandCards()
    {
        if (PlacedCards.Count == 0) return HandCards;

        var firstCard = PlacedCards[0];

        if (TrumpCards.Contains(firstCard))
        {
            var trump = HandCards.Where(c => TrumpCards.Contains(c));

            // Only trump can be placed on trump
            if (trump.Any()) return trump;
            // ... except when player has none
            else return HandCards;
        }
        else
        {
            CardColor color = firstCard.Color;
            var colored = HandCards.Where(c => c.Color == color && !TrumpCards.Contains(c));

            // Same color has to be placed
            if (colored.Any()) return colored;
            // ... except when player has none
            else return HandCards;
        }
    }

    private void SortHandCards()
    {
        HandCards = HandCards.OrderBy(c => TrumpCards.IndexOf(c)).ThenBy(c => c.Color).ThenBy(c => c.Value).ToList();
    }

    private void RemoveHandCard(CardBase card)
    {
        var cardIdx = HandCards.IndexOf(card);

        var newHandCards = HandCards.Take(cardIdx).Concat(HandCards.Skip(cardIdx + 1)).ToList();
        HandCards = newHandCards;
    }

    #region Overrides & Implementations

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string property)
    {
        Debug.WriteLine($"Property {property} of DokoViewModel changed.");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    private readonly Dispatcher _dispatcher;
    public bool IsSynchronized => throw new NotImplementedException();

    public void Invoke(Action action)
    {
        _dispatcher.Invoke(action);
    }

    public void BeginInvoke(Action action)
    {
        _dispatcher.BeginInvoke(action);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _client?.Dispose();
            }

            // TODO: set large fields to null
            _client = null;
            _cardImages.Clear();
            _handCards.Clear();
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
