using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoLib;

/// <summary>
/// An enumeration of Doko announcements.
/// </summary>
public enum Announcement
{
    None,
    Re,
    Contra,
    Under90,
    Under60,
    Under30,
    Black
}

/// <summary>
/// Describes a single round of Doko.
/// </summary>
public class Round
{
    /// <summary>
    /// Describes the results of a round.
    /// </summary>
    public record RoundResult
    {
        /// <summary>
        /// The players that won the round.
        /// </summary>
        public Player[] Winners { get; init; }
        /// <summary>
        /// The total value of the winners.
        /// </summary>
        public int WinnerValue { get; init; }
        /// <summary>
        /// The players that lost the round.
        /// </summary>
        public Player[] Loosers { get; init; }
        /// <summary>
        /// The total value of the loosers.
        /// </summary>
        public int LooserValue { get; init; }
        /// <summary>
        /// The points that were rewarded for the round.
        /// If the round was a solo, corresponds to the asbolute points the opponents will receive.
        /// </summary>
        public int BasePoints { get; init; }

        /// <summary>
        /// Is true if the round was a solo, otherwise false.
        /// </summary>
        public bool IsSolo => Winners.Length == 1 || Loosers.Length == 1;

        /// <summary>
        /// Is true if the Re party won, otherwise false.
        /// </summary>
        public bool RePartyWon { get; init; }

        public RoundResult(IEnumerable<Player> winners, int winnerValue, IEnumerable<Player> loosers, int looserValue, int points, bool rePartyWon)
        {
            Winners = winners.ToArray();
            WinnerValue = winnerValue;
            Loosers = loosers.ToArray();
            LooserValue = looserValue;
            BasePoints = points;
            RePartyWon = rePartyWon;
        }
    }

    /// <summary>
    /// Contains additional information about a Doko round.
    /// </summary>
    public class RoundDescription
    {
        /// <summary>
        /// The ascending ranking of trump cards in the round.
        /// </summary>
        public IList<CardBase> TrumpRanking { get; init; }

        /// <summary>
        /// The active reservation during the round.
        /// </summary>
        public Reservation? ActiveReservation { get; internal set; }

        /// <summary>
        /// The Re party of the round.
        /// </summary>
        public IList<Player> ReParty { get; init; }

        /// <summary>
        /// The additional points of the Re party.
        /// </summary>
        public IList<string> ReAdditionalPoints { get; init; }

        /// <summary>
        /// The announcement of the Re party.
        /// </summary>
        public Announcement ReAnnouncement { get; set; } = Announcement.None;

        /// <summary>
        /// Returns true if the Re party contains only 1 member.
        /// </summary>
        public bool IsSolo => ReParty.Count == 1;

        /// <summary>
        /// The Contra party of the round.
        /// </summary>
        public IList<Player> ContraParty { get; init; }

        /// <summary>
        /// The announcement of the Contra party.
        /// </summary>
        public Announcement ContraAnnouncement { get; set; } = Announcement.None;

        /// <summary>
        /// The additional points of the Contra party.
        /// </summary>
        public IList<string> ContraAdditionalPoints { get; init; }

        internal RoundDescription()
        {
            TrumpRanking = GetDefaultTrumpRanking();
            ReParty = new List<Player>();
            ReAdditionalPoints = new List<string>();
            ContraParty = new List<Player>();
            ContraAdditionalPoints = new List<string>();
        }

        /// <summary>
        /// Returns the default trump ranking.
        /// </summary>
        /// <returns></returns>
        protected static IList<CardBase> GetDefaultTrumpRanking()
        {
            List<CardBase> result = new();

            // Add Damen and Buben
            foreach (string symbol in new[] { "D", "B" })
            {
                foreach (CardColor color in Enum.GetValues<CardColor>())
                {
                    result.Add(CardBase.Existing[color][symbol]);
                }
            }

            // Add remaining Karo
            foreach (string symbol in new[] { "A", "10", "K", "9" })
            {
                result.Add(CardBase.Existing[CardColor.Karo][symbol]);
            }

            // Reverse to list so the rank is ascending
            result.Reverse();
            return result;
        }
    }

    #region Fields

    protected Player[] _players;
    protected Trick[] _finishedTricks;
    protected Player[]? _reParty;
    protected Player[]? _contraParty;

    #endregion

    #region Properties

    /// <summary>
    /// The game the round belongs to.
    /// </summary>
    public Game Game { get; protected set; }

    /// <summary>
    /// The description of the round.
    /// Contains information about the parties, announcements and the card ranking.
    /// </summary>
    public RoundDescription Description { get; protected set; }

    /// <summary>
    /// A list of players in the order they are acting in the round.
    /// </summary>
    public Player[] PlayersInOrder => _players;

    /// <summary>
    /// A list of the already finished tricks.
    /// </summary>
    public IReadOnlyList<Trick> FinishedTricks => _finishedTricks;

    /// <summary>
    /// A flag that determines if the round is running.
    /// </summary>
    public bool IsRunning { get; protected set; } = false;

    /// <summary>
    /// A flag that determines if the round has finished.
    /// </summary>
    public bool IsFinished => Result != null;

    /// <summary>
    /// The result of the round.
    /// Is null while the round hasn't finished.
    /// </summary>
    public RoundResult? Result { get; protected set; } = null;

    #region Trick-related

    /// <summary>
    /// The current trick of the round.
    /// Is null while the round isn't running.
    /// </summary>
    public Trick? CurrentTrick { get; protected set; } = null;

    /// <summary>
    /// The number of the currently played trick.
    /// Is zero if the round hasn't started.
    /// </summary>
    public int CurrentTrickNumber { get; protected set; } = 0;

    #endregion

    #endregion

    /// <summary>
    /// Creates a new round of Doko with players in the given order.
    /// </summary>
    /// <param name="playersInOrder"></param>
    public Round(Game game, IEnumerable<Player> playersInOrder)
    {
        Game = game;
        Description = new();
        _players = playersInOrder.ToArray();
        _finishedTricks = new Trick[12];
    }

    /// <summary>
    /// Starts the round. 
    /// Does nothing if it is already running or was finished before.
    /// </summary>
    public void Start()
    {
        if (IsRunning || IsFinished)
        {
            Log.Warning("The round is already running or has finished. Aborting.");
        }
        IsRunning = true;
        Log.Information("Round started.");

        // Invoke special rules
        Log.Debug("Call OnRoundStarted callback of special rules.");
        Game.SpecialRules.ForEach(rule => rule.OnRoundStarted?.Invoke(this));

        // Give hand cards and perform reservations
        bool giveHandsAgain = true;
        while (giveHandsAgain)
        {
            GiveHandCards();
            // Invoke special rules
            Game.SpecialRules.ForEach(rule => rule.OnHandsGiven?.Invoke(this));

            PerformReservations();
            giveHandsAgain = Description.ActiveReservation?.Type == ReservationType.GiveHandsAgain;
        }
        // Invoke special rules
        Log.Debug("Call OnReservationsPerformed callback of special rules.");
        Game.SpecialRules.ForEach(rule => rule.OnReservationsPerformed?.Invoke(this));

        // Do all 12 tricks
        int currentStartIdx = 0;
        while (CurrentTrickNumber < 12)
        {
            // Determine order of players for the trick
            Player[] playersInOrder = new Player[4];
            Array.Copy(_players, currentStartIdx, playersInOrder, 0, 4 - currentStartIdx);
            Array.Copy(_players, 0, playersInOrder, 4 - currentStartIdx, currentStartIdx);

            // Execute trick
            CurrentTrickNumber++;
            CurrentTrick = new(this, playersInOrder);
            Log.Debug("Created trick {i} with starting player {player}", CurrentTrickNumber, playersInOrder[0].Name);
            
            CurrentTrick.Start();

            // Add trick to finished tricks
            _finishedTricks[CurrentTrickNumber - 1] = CurrentTrick;
            // Invoke special rules
            Log.Debug("Call OnTrickEnded callback of special rules.");
            Game.SpecialRules.ForEach(rule => rule.OnTrickEnded?.Invoke(CurrentTrick));

            // Winner starts next trick
            Player winner = CurrentTrick.Winner!;
            currentStartIdx = Array.IndexOf(_players, winner);
        }
        CurrentTrick = null;

        DetermineResult();
        IsRunning = false;
        Log.Information("Round finished.");
    }

    /// <summary>
    /// Gives hand cards to the players and assigns them to a party according to the
    /// possession of a Kreuz Dame.
    /// </summary>
    protected void GiveHandCards()
    {
        Log.Information("Start giving hand cards.");
        Card[] deck = Card.GetDeckOfCards(this).Shuffle().ToArray();

        CardBase kreuzDame = CardBase.Existing[CardColor.Kreuz]["D"];
        for (int i = 0; i < 4; i++)
        {
            Player player = PlayersInOrder[i];
            Card[] handCards = deck[(12 * i)..(12 * (i + 1))];

            player.ReceiveHandCards(handCards);
            Log.Information("Gave hand cards to player {player}.", player.Name);

            // Assign player to party according to possession of Kreuz Dame
            if (handCards.Any(c => c.Base == kreuzDame))
            {
                Description.ReParty.Add(player);
                Log.Information("Added player {player} to Re party.", player.Name);
            } else
            {
                Description.ContraParty.Add(player);
                Log.Information("Added player {player} to Contra party.", player.Name);
            }
        }

        Log.Information("Finished giving hand cards.");
    }

    /// <summary>
    /// Asks all players for their reservations and applies the highest ranking one.
    /// </summary>
    protected void PerformReservations()
    {
        Log.Information("Starting reservations.");

        List<Reservation> reservations = new();
        for (int i = 0; i < PlayersInOrder.Length; i++)
        {
            Player player = PlayersInOrder[i];
            Reservation? reservation = player.RequestReservation();
            if (reservation is null) Log.Information("Player {player} has no reservation.", player.Name);
            else
            {
                reservations.Add(reservation);
                Log.Information("Player {player} has a reservation of type {type}.", player.Name, reservation.Type);
            }

        }

        Reservation? activeReservation = null;
        foreach (Reservation reservation in reservations)
        {
            if (reservation is null) continue;
            if (!reservation.Player.RequestYesNo("Reveal you reservation?")) continue;

            if (activeReservation is null || reservation.Type > activeReservation.Type)
            {
                activeReservation = reservation;
            }
        }

        Description.ActiveReservation = activeReservation;        
        if (activeReservation is null) Log.Information("No active reservation set.");
        else Log.Information("Active reservation is of type {type} by player{player}.", 
                             activeReservation.Type,
                             activeReservation.Player.Name);

        Log.Information("Finished reservations.");
    }

    /// <summary>
    /// Determines the result of the round.
    /// </summary>
    /// <returns>An object describing the result.</returns>
    protected void DetermineResult()
    {
        Log.Information("Start determining results.");

        int reValue = 0;
        int contraValue = 0;
        // Calculate total value of parties
        foreach(Trick trick in FinishedTricks)
        {
            if (Description.ReParty.Contains(trick.Winner!))
            {
                reValue += trick.Value;
            } else
            {
                contraValue += trick.Value;
            }
        }
        Log.Information("Re party has {value} value.", reValue);
        Log.Information("Contra party has {value} value.", contraValue);

        int rePoints = Description.ReAdditionalPoints.Count;
        int contraPoints = Description.ContraAdditionalPoints.Count;

        int valuePoints = 0;
        int lowerPoints = (reValue > contraValue) ? contraValue : reValue;
        if (lowerPoints < 90) valuePoints++;
        if (lowerPoints < 60) valuePoints++;
        if (lowerPoints < 30) valuePoints++;
        if (lowerPoints == 0) valuePoints++;

        if (reValue > contraValue) rePoints += valuePoints;
        else contraPoints += valuePoints;

        int basePoints = (reValue > contraValue) ? rePoints - contraPoints : contraPoints - rePoints;
        if (reValue > contraValue)
        {
            Result = new(Description.ReParty, reValue, Description.ContraParty, contraValue, basePoints, true);
        } else
        {
            Result = new(Description.ContraParty, contraValue, Description.ReParty, reValue, basePoints, false);
        }

        Log.Information("Finished determining results.");
    }
}
