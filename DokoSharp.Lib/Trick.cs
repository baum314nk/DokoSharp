using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib;

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

    #region Events

    public class CardPlacedEventArgs : EventArgs
    {
        /// <summary>
        /// The player who placed the card.
        /// </summary>
        public Player Player { get; init; }

        /// <summary>
        /// The placed card.
        /// </summary>
        public Card PlacedCard { get; init; }

        public CardPlacedEventArgs(Player player, Card placedCard)
        {
            Player = player;
            PlacedCard = placedCard;
        }

    }
    public delegate void CardPlacedEventHandler(object sender, CardPlacedEventArgs e);
    public event CardPlacedEventHandler? CardPlaced;

    public class TrickFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The player who won the trick.
        /// </summary>
        public Player Winner { get; init; }

        /// <summary>
        /// The value of the trick.
        /// </summary>
        public int Value { get; init; }

        public TrickFinishedEventArgs(Player winner, int value)
        {
            Winner = winner;
            Value = value;
        }

    }
    public delegate void TrickFinishedEventHandler(object sender, TrickFinishedEventArgs e);
    public event TrickFinishedEventHandler? TrickFinished;

    #endregion

    #region Constructor

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

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the trick. 
    /// Does nothing if it is already running or was finished before.
    /// </summary>
    public void Start()
    {
        if (IsRunning || IsFinished)
        {
            Log.Warning("The trick is already running or has finished. Aborting.");
            return;
        }

        IsRunning = true;
        Log.Information("Trick started.");

        // Let each player place a card
        for (int i = 0; i < 4; i++)
        {
            Player player = PlayersInOrder[i];

            Card placedCard = player.PlaceCard(this);
            _cards[i] = placedCard;
            Log.Information("Player {player} placed the card {card}.", player.Name, placedCard);
            CardPlaced?.Invoke(this, new(player, placedCard));
        }

        DetermineWinner();

        IsRunning = false;
        Log.Information("Trick finished.");
        TrickFinished?.Invoke(this, new(Winner!, Value));
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
            if (card.IsTrump)
            {
                Log.Debug("Valid placing. Player {player} can place {card} on {firstCard}.", player.Name, card, firstCard);
                return true;
            }

            // Placing is only valid if player doesn't have any trump cards
            if (!player.HasTrumpCard())
            {
                Log.Debug("Valid placing. Player {player} doesn't have a trump card to place on {firstCard}.", player.Name, firstCard);
                return true;
            }
            else
            {
                Log.Debug("Invalid placing. Player {player} has a trump card to place on on {firstCard}.", player.Name, firstCard);
                return false;
            }
        }
        else // Color of the first card has to be placed
        {
            if (card.Base.Color == firstCard.Base.Color && !card.IsTrump)
            {
                Log.Debug("Valid placing. Player {player} can place {card} on {firstCard}.", player.Name, card, firstCard);
                return true;
            }

            // Placing is only valid if player doesn't have any cards of the required color
            if (!player.HasColoredCard(firstCard.Base.Color))
            {
                Log.Debug("Valid placing. Player {player} doesn't have a {color} card to place on {firstCard}.", player.Name, firstCard.Base.Color, firstCard);
                return true;
            }
            else
            {
                Log.Debug("Invalid placing. Player {player} has a {color} card to place on {firstCard}.", player.Name, firstCard.Base.Color, firstCard);
                return false;
            }
        }
    }

    #endregion

    #region Protected & Private Methods

    /// <summary>
    /// Determines the winner of the trick and set the <see cref="Winner"/> property accordingly.
    /// </summary>
    protected void DetermineWinner()
    {
        int winnerIdx = 0;
        Card winnerCard = PlacedCards[0];
        for (int i = 1; i < 4; i++)
        {
            Card card = PlacedCards[i];
            if (winnerCard.IsTrump)
            {
                if (card.TrumpRank > winnerCard.TrumpRank)
                {
                    winnerCard = card;
                    winnerIdx = i;
                }
            } else
            {
                if (card.IsTrump)
                {
                    winnerCard = card;
                    winnerIdx = i;
                } else if(card.Base.Color == winnerCard.Base.Color && card.Base.Value > winnerCard.Base.Value)
                {
                    winnerCard = card;
                    winnerIdx = i;
                }
            }
        }

        Winner = PlayersInOrder[winnerIdx];
        Log.Information("Player {player} won the trick with a {card}.", Winner.Name, winnerCard);
    }

    #endregion
}
