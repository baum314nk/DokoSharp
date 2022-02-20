using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib;

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
/// Describes the results of a round.
/// </summary>
public record RoundResult
{
    /// <summary>
    /// Is true if the Re party won, otherwise false.
    /// </summary>
    public bool RePartyWon { get; init; }

    /// <summary>
    /// The total value of the Re party.
    /// </summary>
    public int ReValue { get; init; }

    /// <summary>
    /// The total value of the Contra party.
    /// </summary>
    public int ContraValue { get; init; }

    /// <summary>
    /// The points that were rewarded for the round.
    /// If the round was a solo, corresponds to the asbolute points the opponents will receive.
    /// </summary>
    public int BasePoints { get; init; }

    /// <summary>
    /// Is true if the round was a solo, otherwise false.
    /// </summary>
    public bool IsSolo { get; init; }

    public RoundResult(bool rePartyWon, int reValue, int contraValue, int basePoints, bool isSolo)
    {
        RePartyWon = rePartyWon;
        ReValue = reValue;
        ContraValue = contraValue;
        BasePoints = basePoints;
        IsSolo = isSolo;
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

        // Add Ober and Unter
        foreach (string symbol in new[] { "O", "U" })
        {
            foreach (CardColor color in Enum.GetValues<CardColor>())
            {
                result.Add(CardBase.Existing[color][symbol]);
            }
        }

        // Add remaining Schell
        foreach (string symbol in new[] { "A", "10", "K", "9" })
        {
            result.Add(CardBase.Existing[CardColor.Schell][symbol]);
        }

        // Reverse to list so the rank is ascending
        result.Reverse();
        return result;
    }
}

/// <summary>
/// Describes a single round of Doko.
/// </summary>
public class Round
{
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

    #region Events

    public class RoundStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The ranking of trump cards in the round.
        /// </summary>
        public IEnumerable<CardBase> TrumpRanking { get; init; }

        public RoundStartedEventArgs(IEnumerable<CardBase> trumpRanking)
        {
            TrumpRanking = trumpRanking;
        }

    }
    public delegate void RoundStartedEventHandler(object sender, RoundStartedEventArgs e);
    public event RoundStartedEventHandler? RoundStarted;

    public class ReservationsPerformedEventArgs : EventArgs
    {
        /// <summary>
        /// The active reservation of the current game.
        /// </summary>
        public Reservation? ActiveReservation { get; init; }

        public ReservationsPerformedEventArgs(Reservation? activeReservation)
        {
            ActiveReservation = activeReservation;
        }

    }
    public delegate void ReservationsPerformedEventHandler(object sender, ReservationsPerformedEventArgs e);
    public event ReservationsPerformedEventHandler? ReservationsPerformed;

    public class RegistrationsAppliedEventArgs : EventArgs
    {
        /// <summary>
        /// The ranking of trump cards in the round.
        /// </summary>
        public IEnumerable<CardBase> TrumpRanking { get; init; }

        public RegistrationsAppliedEventArgs(IEnumerable<CardBase> trumpRanking)
        {
            TrumpRanking = trumpRanking;
        }
    }
    public delegate void RegistrationsAppliedEventHandler(object sender, RegistrationsAppliedEventArgs e);
    public event RegistrationsAppliedEventHandler? RegistrationsApplied;

    public class TrickCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The number of the trick in the current round.
        /// </summary>
        public int TrickNumber { get; init; }

        /// <summary>
        /// The created trick instance.
        /// </summary>
        public Trick Trick { get; init; }

        public TrickCreatedEventArgs(int trickNumber, Trick trick)
        {
            TrickNumber = trickNumber;
            Trick = trick;
        }

    }
    public delegate void TrickCreatedEventHandler(object sender, TrickCreatedEventArgs e);
    public event TrickCreatedEventHandler? TrickCreated;

    public class RoundFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The result of the round.
        /// </summary>
        public RoundResult Result { get; init; }

        public RoundFinishedEventArgs(RoundResult result)
        {
            Result = result;
        }

    }
    public delegate void RoundFinishedEventHandler(object sender, RoundFinishedEventArgs e);
    public event RoundFinishedEventHandler? RoundFinished;


    #endregion

    #region Constructor

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

    #endregion

    #region Public Methods

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

        // Invoke RoundStarted event
        RoundStarted?.Invoke(this, new(Description.TrumpRanking));

        // Give hand cards and perform reservations
        bool giveHandsAgain = true;
        while (giveHandsAgain)
        {
            GiveHandCards();
            // Invoke special rules
            Game.SpecialRules.ForEach(rule => rule.OnHandsGiven?.Invoke(this));

            PerformReservations();
            giveHandsAgain = Description.ActiveReservation?.Type == ReservationType.GiveHandsAgain;
            // Invoke ReservationsPerformed event
            ReservationsPerformed?.Invoke(this, new(Description.ActiveReservation));
        }
        // Invoke special rules
        Log.Debug("Call OnApplyRegistrations callback of special rules.");
        Game.SpecialRules.ForEach(rule => rule.OnApplyRegistrations?.Invoke(this));
        RegistrationsApplied?.Invoke(this, new(Description.TrumpRanking));

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
            TrickCreated?.Invoke(this, new(CurrentTrickNumber, CurrentTrick));

            CurrentTrick.Start();

            // Add trick to finished tricks
            _finishedTricks[CurrentTrickNumber - 1] = CurrentTrick;
            // Invoke special rules
            Log.Debug("Call OnTrickEnded callback of special rules.");
            Game.SpecialRules.ForEach(rule => rule.OnTrickFinished?.Invoke(CurrentTrick));

            // Winner starts next trick
            Player winner = CurrentTrick.Winner!;
            currentStartIdx = Array.IndexOf(_players, winner);
        }
        CurrentTrick = null;

        DetermineResult();
        IsRunning = false;
        Log.Information("Round finished.");
        RoundFinished?.Invoke(this, new(Result!));
    }

    #endregion

    #region Private & Protected Methods

    /// <summary>
    /// Gives hand cards to the players and assigns them to a party according to the
    /// possession of a Eichel Ober.
    /// </summary>
    protected void GiveHandCards()
    {
        Log.Information("Start giving hand cards.");
        Card[] deck = Card.GetDeckOfCards(this).Shuffle().ToArray();

        CardBase eichelOber = CardBase.Existing[CardColor.Eichel]["O"];
        for (int i = 0; i < 4; i++)
        {
            Player player = PlayersInOrder[i];
            Card[] handCards = deck[(12 * i)..(12 * (i + 1))];

            player.ReceiveCards(handCards, true);
            Log.Information("Gave hand cards to player {player}.", player.Name);

            // Assign player to party according to possession of Eichel Ober
            if (handCards.Any(c => c.Base == eichelOber))
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
            Reservation? reservation = player.AnnounceReservation();
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
            if (!reservation.Player.DecideYesNo("Reveal you reservation?")) continue;

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
            Result = new(true, reValue, contraValue, basePoints, Description.IsSolo);
        } else
        {
            Result = new(false, reValue, contraValue, basePoints, Description.IsSolo);
        }

        Log.Information("Finished determining results.");
    }

    #endregion
}
