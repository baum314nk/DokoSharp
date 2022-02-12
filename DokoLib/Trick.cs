using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoLib;

/// <summary>
/// Describes a trick in a round of Doko.
/// </summary>
public class Trick
{
    #region Fields

    protected Player[] _players;
    protected Card?[] _cards;

    #endregion

    #region Properties

    /// <summary>
    /// The round the trick belongs to.
    /// </summary>
    public Round Round { get; protected set; }

    /// <summary>
    /// A list of players in the order they are acting in the trick.
    /// </summary>
    public IReadOnlyList<Player> PlayersInOrder => _players;

    /// <summary>
    /// The winner of the trick.
    /// </summary>
    public Player? Winner { get; protected set; }

    /// <summary>
    /// The cards that were placed in the trick.
    /// </summary>
    public IReadOnlyList<Card> PlacedCards => _cards.TakeWhile(c => c != null).Cast<Card>().ToList();

    /// <summary>
    /// A flag that determines if the trick is running.
    /// </summary>
    public bool IsRunning { get; protected set; } = false;

    /// <summary>
    /// A flag that determines if the trick has finished.
    /// </summary>
    public bool IsFinished => Winner != null;

    /// <summary>
    /// The value of the trick.
    /// Corresponds to the sum of values of the placed cards.
    /// </summary>
    public int Value => PlacedCards.Sum(c => c.Base.Value);

    #endregion

    /// <summary>
    /// Creates a new trick with the players in the given order.
    /// </summary>
    /// <param name="playersInOrder"></param>
    public Trick(Round round, IEnumerable<Player> playersInOrder)
    {
        Round = round;
        _players = playersInOrder.ToArray();
        _cards = new Card?[4];
    }

    /// <summary>
    /// Starts the trick. 
    /// Does nothing if it is already running or was finished before.
    /// </summary>
    public void Start()
    {
        if (IsRunning || IsFinished) return;
        IsRunning = true;

        // Let each player place a card
        for (int i = 0; i < 4; i++)
        {
            Card placedCard = PlayersInOrder[i].PlaceCard(this);
            _cards[i] = placedCard;
        }

        IsRunning = false;
    }

    /// <summary>
    /// Validates whether the given player can place the given card into the trick.
    /// </summary>
    public bool ValidatePlacing(Player player, Card card)
    {
        // First card can be anything
        if (PlacedCards.Count == 0) return true;
        
        var firstCard = PlacedCards[0];

        // Trump has to be placed
        if (firstCard.IsTrump)
        {
            if (card.IsTrump) return true;

            // Placing is only valid if player doesn't have any trump cards
            return !player.HasTrumpCard();
        } else // Color of the first card has to be placed
        {
            if (card.Base.Color == firstCard.Base.Color) return true;

            // Placing is only valid if player doesn't have any cards of the required color
            return !player.HasColoredCard(firstCard.Base.Color);
        }
    }
}
