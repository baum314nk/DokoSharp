using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib;

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
    /// The points of the Re party.
    /// </summary>
    public IList<string> RePoints { get; init; }

    /// <summary>
    /// The total value of the Contra party.
    /// </summary>
    public int ContraValue { get; init; }

    /// <summary>
    /// The points of the Contra party.
    /// </summary>
    public IList<string> ContraPoints { get; init; }

    /// <summary>
    /// The points that were rewarded for the round.
    /// If the round was a solo, corresponds to the points each opponent will receive.
    /// </summary>
    public int BasePoints { get; init; }

    public RoundResult(bool rePartyWon, int reValue, IList<string> rePoints, int contraValue, IList<string> contraPoints, int basePoints)
    {
        RePartyWon = rePartyWon;
        ReValue = reValue;
        RePoints = rePoints;
        ContraValue = contraValue;
        ContraPoints = contraPoints;
        BasePoints = basePoints;
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
    public IList<CardBase> TrumpRanking { get; set; }

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
    /// The number of the last trick in which the Re party can make an announcement.
    /// </summary>
    public int LastReAnnouncementNumber { get; set; } = 2;

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
    /// The number of the last trick in which the Contra party can make an announcement.
    /// </summary>
    public int LastContraAnnouncementNumber { get; set; } = 2;

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

        // Add Schell
        foreach (string symbol in new[] { "9", "K", "10", "A" })
        {
            result.Add(CardBase.Existing[CardColor.Schell][symbol]);
        }

        // Add Ober and Unter
        foreach (string symbol in new[] { "U", "O" })
        {
            foreach (CardColor color in Enum.GetValues<CardColor>())
            {
                result.Add(CardBase.Existing[color][symbol]);
            }
        }

        // Reverse to list so the rank is ascending
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

    public class AnnouncementMadeEventArgs : EventArgs
    {
        /// <summary>
        /// The player who made the announcement.
        /// </summary>
        public Player Player { get; init; }

        /// <summary>
        /// The announcement that has been made.
        /// </summary>
        public Announcement Announcement { get; init; }

        public AnnouncementMadeEventArgs(Player player, Announcement announcement)
        {
            Player = player;
            Announcement = announcement;
        }
    }
    public delegate void AnnouncementMadeEventHandler(object sender, AnnouncementMadeEventArgs e);
    public event AnnouncementMadeEventHandler? AnnouncementMade;

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
        Game.Rules.ForEach(rule => rule.OnRoundStarted?.Invoke(this));

        // Invoke RoundStarted event
        RoundStarted?.Invoke(this, new(Description.TrumpRanking));

        // Give hand cards and perform reservations
        while (true)
        {
            GiveHandCards();
            // Invoke special rules
            Game.Rules.ForEach(rule => rule.OnHandsGiven?.Invoke(this));

            PerformReservations();
            if (Description.ActiveReservation?.Name == "Einmischen") continue;

            // Invoke special rules
            Game.Rules.ForEach(rule => rule.OnReservationsPerformed?.Invoke(this, Description.ActiveReservation));
            // Invoke ReservationsPerformed event
            ReservationsPerformed?.Invoke(this, new(Description.ActiveReservation));
            break;
        }
        // Invoke special rules
        Log.Debug("Call OnApplyRegistrations callback of special rules.");
        Game.Rules.ForEach(rule => rule.OnApplyRegistrations?.Invoke(this));
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
            Game.Rules.ForEach(rule => rule.OnTrickFinished?.Invoke(CurrentTrick));

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

    /// <summary>
    /// Validates whether the given player can make the given announcement.
    /// </summary>
    internal bool ValidateAnnouncement(Player player, Announcement announcement)
    {
        if (!player.CanMakeAnnouncement)
        {
            Log.Debug("Invalid announcement. Player {player} isn't allowed to make an announcement.", player.Name);
            return false;
        }

        if (player.IsReParty)
        {
            if (announcement == Announcement.Contra)
            {
                Log.Debug("Invalid announcement. Player {player} is part of the Re party and can't make a Contra announcement.", player.Name);
                return false;
            }
            if (announcement <= Description.ReAnnouncement)
            {
                Log.Debug("Invalid announcement. The current announcement of player {player}s party is already equal or higher.", player.Name);
                return false;
            }
        }
        else
        {
            if (announcement == Announcement.Re)
            {
                Log.Debug("Invalid announcement. Player {player} is part of the Contra party and can't make a Re announcement.", player.Name);
                return false;
            }
            if (announcement <= Description.ContraAnnouncement)
            {
                Log.Debug("Invalid announcement. The current announcement of player {player}s party is already equal or higher.", player.Name);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Lets a player make an announcement.
    /// If other announcements are between the current and the announcement to be made,
    /// to announcements in between are made first.
    /// Does nothing if the announcement is none or invalid.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="announcement"></param>
    internal void MakeAnnouncement(Player player, Announcement announcement)
    {
        Announcement oldAnnouncement = player.IsReParty ? Description.ReAnnouncement : Description.ContraAnnouncement;
        for (var next = oldAnnouncement + 1; next <= announcement; next++)
        {
            // Skip Re entry if player is Contra party and next announcement should be Contra
            if (next == Announcement.Re && !player.IsReParty) next++;
            // Skip Contra entry if player is Re party and next announcement should be Under90
            if (next == Announcement.Contra && player.IsReParty) next++;

            // Set next announcement and increment number of lastest announcement trick
            if (player.IsReParty)
            {
                Description.ReAnnouncement = next;
                Description.LastReAnnouncementNumber++;
            }
            else
            {
                Description.ContraAnnouncement = next;
                Description.LastContraAnnouncementNumber++;
            }

            Log.Information("Player {player} made the announcement {ann}.", player.Name, next);
            // Invoke AnnouncementMade event
            AnnouncementMade?.Invoke(this, new(player, next));
        }
    }

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
            }
            else
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
            Reservation? reservation = player.DecideReservation();
            if (reservation is null) Log.Information("Player {player} has no reservation.", player.Name);
            else
            {
                reservations.Add(reservation);
                Log.Information("Player {player} has a reservation {name}.", player.Name, reservation.Name);
            }

        }

        Reservation? activeReservation = null;
        foreach (Reservation reservation in reservations)
        {
            // Ask player if he wants to reveal his reservation
            if (!reservation.Player.DecideYesNo("Reveal you reservation?")) continue;

            // Select reservation if its ranked higher than the current one
            if (activeReservation is null || reservation.Rank > activeReservation.Rank)
            {
                activeReservation = reservation;
            }
        }

        Description.ActiveReservation = activeReservation;
        if (activeReservation is null) Log.Information("No active reservation set.");
        else Log.Information("Active reservation is {name} by player{player}.",
                             activeReservation.Name,
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
        foreach (Trick trick in FinishedTricks)
        {
            if (Description.ReParty.Contains(trick.Winner!))
            {
                reValue += trick.Value;
            }
            else
            {
                contraValue += trick.Value;
            }
        }
        Log.Information("Re party has {value} value.", reValue);
        Log.Information("Contra party has {value} value.", contraValue);

        List<string> rePoints = new();
        List<string> contraPoints = new();

        // Determine value points
        bool reMissedAnnouncement = false;
        bool contraMissedAnnouncement = false;
        for (Announcement announcement = Announcement.Under90; announcement <= Announcement.Black; announcement++)
        {
            var value = (announcement != Announcement.Black) ? (6 - (int)announcement) * 30 : 1;
            var valuePoint = announcement.GetName()!;

            // Check Re value point
            if (contraValue < value) rePoints.Add(valuePoint);
            // Check Contra value point
            if (reValue < value) contraPoints.Add(valuePoint);

            // Check Re announcement
            if (Description.ReAnnouncement >= announcement)
            {
                if (contraValue < value) rePoints.Add($"{valuePoint} Angesagt");
                else
                {
                    contraPoints.Add($"{valuePoint} Abgesagt");
                    reMissedAnnouncement = true;
                }
            }
            // Check Contra announcement
            if (Description.ContraAnnouncement >= announcement)
            {
                if (reValue < value) contraPoints.Add($"{valuePoint} Angesagt");
                else
                {
                    rePoints.Add($"{valuePoint} Abgesagt");
                    contraMissedAnnouncement = true;
                }
            }
        }

        // Find out which party won
        bool reWins = false;
        if (reMissedAnnouncement && contraMissedAnnouncement)
        {
            rePoints.Clear();
            contraPoints.Clear();
        }
        else if (reMissedAnnouncement)
        {
            contraPoints.Add("Gewonnen");
            contraPoints.Add("Gegen die Alten");
            rePoints.Clear();
        }
        else if (contraMissedAnnouncement)
        {
            rePoints.Add("Gewonnen");
            contraPoints.Clear();
            reWins = true;
        }
        else if (reValue > 120)
        {
            rePoints.Add("Gewonnen");
            contraPoints.Clear();
            reWins = true;
        }
        else
        {
            contraPoints.Add("Gewonnen");
            contraPoints.Add("Gegen die Alten");
            rePoints.Clear();
        }

        // Check Re announcement
        if (Description.ReAnnouncement >= Announcement.Re)
        {
            if (reWins)
            {
                rePoints.Add("Re");
                rePoints.Add("Re");
            }
            else
            {
                contraPoints.Add("Re Abgesagt");
                contraPoints.Add("Re Abgesagt");
            }
        }
        // Check Contra announcement
        if (Description.ContraAnnouncement >= Announcement.Contra)
        {
            if (!reWins)
            {
                contraPoints.Add("Contra");
                contraPoints.Add("Contra");
            }
            else
            {
                rePoints.Add("Contra Abgesagt");
                rePoints.Add("Contra Abgesagt");
            }
        }

        // Add additional points
        rePoints.AddRange(Description.ReAdditionalPoints);
        contraPoints.AddRange(Description.ContraAdditionalPoints);

        int basePoints = (reWins ? 1 : -1) * (rePoints.Count - contraPoints.Count);
        Result = new(reValue > contraValue, reValue, rePoints, contraValue, contraPoints, basePoints);

        Log.Information("Finished determining results.");
    }

    #endregion
}
