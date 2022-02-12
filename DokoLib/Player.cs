using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoLib;

/// <summary>
/// Describes a Doko player.
/// </summary>
public class Player
{
    #region Fields

    protected List<Card>? _handCards;

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
    public IReadOnlyList<Card>? HandCards => _handCards;

    #endregion

    /// <summary>
    /// Creates a new player with the given name.
    /// </summary>
    /// <param name="name"></param>
    public Player(Game game, string name)
    {
        Game = game;
        Name = name;
    }

    /// <summary>
    /// Lets the player receive hand cards.
    /// </summary>
    public void ReceiveHandCards(IEnumerable<Card> handCards)
    {
        _handCards = handCards.ToList();
    }

    /// <summary>
    /// Returns true if the player has at least one trump card in hand.
    /// </summary>
    /// <returns></returns>
    public bool HasTrumpCard()
    {
        return HandCards?.Any(c => c.IsTrump) ?? false;
    }

    /// <summary>
    /// Returns true if the player has at least one card of the given color in hand.
    /// Notice that trump cards aren't considered colored.
    /// </summary>
    /// <returns></returns>
    public bool HasColoredCard(CardColor color)
    {
        return HandCards?.Any(c => c.Base.Color == color && !c.IsTrump) ?? false;
    }

    /// <summary>
    /// Returns the tricks the player has won in the current round.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Trick> GetTricks()
    {
        if (Game.CurrentRound is null) throw new Exception("Can't find the tricks of a player while no round is active.");

        return Game.CurrentRound.FinishedTricks.Where(trick => trick.Winner == this);
    }

    /// <summary>
    /// Lets the player place a card into a trick.
    /// </summary>
    /// <param name="placedCards">The cards that have been already placed in the trick.</param>
    /// <returns>The chosen hand card.</returns>
    public Card PlaceCard(Trick trick)
    {
        if (HandCards is null) throw new Exception("Player can't place a card because his hand is empty.");

        Card? selectedCard = null;
        // TODO: Remove
        int i = 0;
        while(selectedCard is null)
        {
            Card card = HandCards[i++];

            if (trick.ValidatePlacing(this, card))
            {
                selectedCard = card;
            }
        }

        // Remove card from hand and return it
        _handCards!.Remove(selectedCard);
        return selectedCard;
    }

    /// <summary>
    /// Requests a yes-no-decision from the player.
    /// Returns true if the player answered yes, otherwise false.
    /// </summary>
    public bool RequestYesNo(string requestText)
    {
        return false;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Requests a reservation from the player.
    /// </summary>
    public Reservation? RequestReservation()
    {
        return null;
    }
}
