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
        }

        _cards.AddRange(receivedCards);
        Log.Debug("Player {player} received the cards: {cards}.", Name, receivedCards);
        _controller.SignalReceivedCards(this, receivedCards);
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

        Card? selectedCard = null;
        while (selectedCard is null)
        {
            Card card = _controller.RequestPlaceCard(this, trick);

            if (trick.ValidatePlacing(this, card))
            {
                selectedCard = card;
            }
        }

        // Remove card from hand and return it
        _cards!.Remove(selectedCard);
        return selectedCard;
    }

    /// <summary>
    /// Requests a given amount of hand card from the player.
    /// The cards are removed from his hand.
    /// </summary>
    /// <returns>The chosen hand card.</returns>
    public IEnumerable<Card> DropCards(int amount = 1)
    {
        if (Cards is null) throw new Exception("Player can't hand out a card because his hand is empty.");
        if (Cards.Count < amount) throw new Exception($"Player can't hand out {amount} cards because he only has {Cards!.Count} in hand.");

        Card[] selectedCards = new Card[amount];

        for (int i = 0; i < amount; i++)
        {
            Card card = _controller.RequestCard(this);
            selectedCards[i] = card;
        }

        // Remove cards from hand and return them
        selectedCards.ForEach(c => _cards!.Remove(c));

        Log.Debug("Player {player} dropped the cards: {cards}.", Name, selectedCards);
        return selectedCards;
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
    /// Requests a reservation from the player.
    /// </summary>
    public Reservation? AnnounceReservation()
    {
        return _controller.RequestReservation(this);
    }

    #endregion
}
