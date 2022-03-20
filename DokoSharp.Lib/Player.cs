using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib;

/// <summary>
/// Describes a Doko player.
/// </summary>
public class Player : IIdentifiable
{
    #region Fields

    protected List<Card> _cards;
    protected IPlayerController _controller;

    #endregion

    #region Properties

    /// <summary>
    /// The game the player belongs to.
    /// </summary>
    public Game Game { get; protected set; }

    /// <summary>
    /// The name of the player.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// The hand cards of the player.
    /// </summary>
    public IReadOnlyList<Card> Cards => _cards;

    public string Identifier => Name;

    /// <summary>
    /// True if player is part of the Re party in the current round.
    /// Is always false if no current round exists.
    /// </summary>
    public bool IsReParty => Game.CurrentRound?.Description.ReParty.Contains(this) ?? false;

    /// <summary>
    /// True if player can make a valid announcement in the current round.
    /// Is always false if no current round exists.
    /// </summary>
    public bool CanMakeAnnouncement
    {
        get
        {
            var round = Game.CurrentRound;
            if (round == null) return false;

            // Can't make announcements after latest allowed trick
            var lastAnnouncementNumber = IsReParty ? round.Description.LastReAnnouncementNumber : round.Description.LastContraAnnouncementNumber;
            if (lastAnnouncementNumber < round.CurrentTrickNumber) return false;

            // Can only make an announcement if the highest announcement has already been made 
            var currentAnnouncement = IsReParty ? round.Description.ReAnnouncement : round.Description.ContraAnnouncement;
            return currentAnnouncement < Announcement.Black;
        }
    }

    #endregion

    /// <summary>
    /// Creates a new player with the given name.
    /// </summary>
    /// <param name="name"></param>
    public Player(IPlayerController controller, Game game, string name)
    {
        _controller = controller;
        Game = game;
        Name = name;
        _cards = new List<Card>();
    }

    #region Public Methods

    /// <summary>
    /// Lets the player receive hand cards.
    /// Additionaly clears old hand before if requested.
    /// </summary>
    public void ReceiveCards(IEnumerable<Card> receivedCards, bool clearOldCards = false)
    {
        if (clearOldCards)
        {
            _cards.Clear();
            Log.Debug("Cleared old cards of player {player}.", Name);
        }

        _cards.AddRange(receivedCards);
        Log.Debug("Player {player} received the cards: {cards}.", Name, receivedCards);
        _controller.SignalReceivedCards(this, receivedCards, clearOldCards);
    }

    /// <summary>
    /// Removes the given cards from the players hand.
    /// Throws an exception if one of the cards to be removed isn't
    /// in the players hand.
    /// </summary>
    /// <param name="removedCards"></param>
    public void RemoveCards(IEnumerable<Card> removedCards)
    {
        // Remove cards from hand
        foreach (Card card in removedCards)
        {
            if (!_cards!.Remove(card))
            {
                throw new ArgumentException($"The card {card} doesn't exist in player {Identifier}s hand.");
            }
        }
        Log.Debug("Player {player} removed the cards: {cards}.", Name, removedCards);
        _controller.SignalRemovedCards(this, removedCards);
    }

    /// <summary>
    /// Returns true if the player has at least one trump card in hand.
    /// </summary>
    /// <returns></returns>
    public bool HasTrumpCard()
    {
        return Cards?.Any(c => c.IsTrump) ?? false;
    }

    /// <summary>
    /// Returns true if the player has at least one card of the given color in hand.
    /// Notice that trump cards aren't considered colored.
    /// </summary>
    /// <returns></returns>
    public bool HasColoredCard(CardColor color)
    {
        return Cards?.Any(c => c.Base.Color == color && !c.IsTrump) ?? false;
    }

    /// <summary>
    /// Returns the tricks the player has won in the given round.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Trick> GetTricks(Round round)
    {
        if (round.Game != Game) throw new Exception("Can't find the tricks of a player in a different game.");

        return round.FinishedTricks.Where(trick => trick.Winner == this);
    }

    /// <summary>
    /// Lets the player place a card into a trick.
    /// </summary>
    /// <param name="placedCards">The cards that have been already placed in the trick.</param>
    /// <returns>The chosen hand card.</returns>
    public Card PlaceCard(Trick trick)
    {
        if (Cards is null) throw new Exception("Player can't place a card because his hand is empty.");

        Card? selectedCard;
        Announcement announcement;
        var canMakeAnnouncement = CanMakeAnnouncement;
        while (true)
        {
            var result = _controller.RequestPlaceCard(this, trick, canMakeAnnouncement);

            // Validate placing
            if (!trick.ValidatePlacing(this, result.Item1)) continue;
            // Validate annonucement
            if (canMakeAnnouncement && result.Item2 != Announcement.None && !trick.Round.ValidateAnnouncement(this, result.Item2)) continue;

            selectedCard = result.Item1;
            announcement = result.Item2;
            break;
        }

        // Make announcement
        if (canMakeAnnouncement && announcement != Announcement.None)
        {
            trick.Round.MakeAnnouncement(this, announcement);
        }

        // Remove card from hand and return it
        _cards!.Remove(selectedCard);
        return selectedCard;
    }

    /// <summary>
    /// Requests a given amount of hand card from the player.
    /// </summary>
    /// <returns>The chosen hand cards.</returns>
    public IEnumerable<Card> DecideCards(string requestText, int amount = 1)
    {
        if (Cards is null) throw new Exception("Player can't hand out a card because his hand is empty.");
        if (Cards.Count < amount) throw new Exception($"Player can't hand out {amount} cards because he only has {Cards!.Count} in hand.");

        return _controller.RequestCards(this, amount, requestText);
    }

    /// <summary>
    /// Requests a yes-no-decision from the player.
    /// Returns true if the player answered yes, otherwise false.
    /// </summary>
    public bool DecideYesNo(string requestText)
    {
        return _controller.RequestYesNo(this, requestText);
    }

    /// <summary>
    /// Lets the player decide a color.
    /// </summary>
    public CardColor DecideColor(string requestText)
    {
        return _controller.RequestColor(this, requestText);
    }

    /// <summary>
    /// Requests a reservation from the player.
    /// </summary>
    public Reservation? DecideReservation()
    {
        // Determine possible reservations
        var possibilities = new List<string>();
        foreach (var kv in Game.ReservationFactories)
        {
            if (kv.Value.CheckPredicate(Cards))
            {
                possibilities.Add(kv.Key);
            }
        }

        // Let controller decide reservation
        var name = _controller.RequestReservation(this, possibilities);

        return (name != null) ? Game.ReservationFactories[name].Create(this) : null;
    }

    #endregion
}
