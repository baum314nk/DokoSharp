using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokoSharp.Lib.Rulings;
using Serilog;

namespace DokoSharp.Lib;

/// <summary>
/// Describes a game of Doko.
/// </summary>
public class Game
{
    #region Fields

    protected Dictionary<Player, int> _points;
    protected Player[] _players;
    protected List<Round> _finishedRounds;

    #endregion

    #region Properties

    /// <summary>
    /// The special rules that apply to the game.
    /// </summary>
    public IList<Rule> Rules { get; set; }

    /// <summary>
    /// A mapping of possible reservations the players can have to a list of reservation factories.
    /// </summary>
    public IDictionary<string, ReservationFactory> ReservationFactories { get; protected set; }

    /// <summary>
    /// The table of points.
    /// </summary>
    public IReadOnlyDictionary<Player, int> Points => _points;

    /// <summary>
    /// The list of players.
    /// </summary>
    public IReadOnlyList<Player> Players => _players;

    /// <summary>
    /// The finished rounds.
    /// </summary>
    public IReadOnlyList<Round> FinishedRounds => _finishedRounds;

    #region Round-related

    /// <summary>
    /// The current round of the game.
    /// Is null while the game isn't running.
    /// </summary>
    public Round? CurrentRound { get; protected set; } = null;

    /// <summary>
    /// The number of the currently played round.
    /// Is zero if the game hasn't started.
    /// </summary>
    public int CurrentRoundNumber { get; protected set; } = 0;

    /// <summary>
    /// The number of rounds that should be played.
    /// Is zero if the game hasn't started.
    /// </summary>
    public int NumberOfRounds { get; protected set; } = 0;

    #endregion

    /// <summary>
    /// A flag that determines if the game is running.
    /// </summary>
    public bool IsRunning { get; protected set; } = false;

    /// <summary>
    /// A flag that determines if the game has finished.
    /// </summary>
    public bool IsFinished { get; protected set; } = false;

    #endregion

    #region Events

    public class GameStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The number of rounds to play.
        /// </summary>
        public int NumberOfRounds { get; init; }

        public GameStartedEventArgs(int numOfRounds)
        {
            NumberOfRounds = numOfRounds;
        }

    }
    public delegate void GameStartedEventHandler(object sender, GameStartedEventArgs e);
    public event GameStartedEventHandler? GameStarted;

    public class RoundCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The number of the round in the current game.
        /// </summary>
        public int RoundNumber { get; init; }

        /// <summary>
        /// The created round instance..
        /// </summary>
        public Round Round { get; init; }

        public RoundCreatedEventArgs(int roundNumber, Round round)
        {
            RoundNumber = roundNumber;
            Round = round;
        }

    }
    public delegate void RoundCreatedEventHandler(object sender, RoundCreatedEventArgs e);
    public event RoundCreatedEventHandler? RoundCreated;

    public class PointsUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// The changes made to the player points.
        /// </summary>
        public IEnumerable<KeyValuePair<Player, int>> PointChanges { get; init; }

        public PointsUpdatedEventArgs(IEnumerable<KeyValuePair<Player, int>> pointChanges)
        {
            PointChanges = pointChanges;
        }

    }
    public delegate void PointsUpdatedEventHandler(object sender, PointsUpdatedEventArgs e);
    public event PointsUpdatedEventHandler? PointsUpdated;

    public class GameFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The ranking of the players.
        /// </summary>
        public IEnumerable<Player> Ranking { get; init; }

        public GameFinishedEventArgs(IEnumerable<Player> ranking)
        {
            Ranking = ranking;
        }

    }
    public delegate void GameFinishedEventHandler(object sender, GameFinishedEventArgs e);
    public event GameFinishedEventHandler? GameFinished;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new game with the given players.
    /// The players will sit in the order they were passed to this method.
    /// </summary>
    public Game(
        Tuple<string, IPlayerController>[] players,
        IEnumerable<Rule> rules)
    {
        if (players.Length != 4) throw new ArgumentException("Exactly 4 player names and associated controllers must be provided.", nameof(players));
        if (players.Select(p => p.Item1).ToHashSet().Count != 4) throw new ArgumentException("All players must have different names.", nameof(players));

        Rules = rules.ToArray();
        ReservationFactories = new Dictionary<string, ReservationFactory>();
        _players = players.Select(p => new Player(p.Item2, this, p.Item1)).ToList().Shuffle().ToArray();
        _points = Players.ToDictionary(p => p, _ => 0);
        _finishedRounds = new();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the game.
    /// Does nothing if it is already running.
    /// </summary>
    public void Start(int numOfRounds = 4)
    {
        if (IsRunning || IsFinished)
        {
            Log.Warning("The game is already running or has finished. Aborting.");
        }

        NumberOfRounds = numOfRounds;
        IsRunning = true;
        Log.Information("Game started.");
        GameStarted?.Invoke(this, new(NumberOfRounds));

        // Invoke special rules
        Log.Debug("Call OnGameStarted callback of special rules.");
        Rules.ForEach(rule => rule.OnGameStarted?.Invoke(this));

        // Game loop
        int currentStartIdx = 0;
        while (CurrentRoundNumber != NumberOfRounds)
        {
            // Determine order of players for the round
            Player[] playersInOrder = new Player[4];
            Array.Copy(_players, currentStartIdx, playersInOrder, 0, 4 - currentStartIdx);
            Array.Copy(_players, 0, playersInOrder, 4 - currentStartIdx, currentStartIdx);

            // Create round and increase round number
            CurrentRoundNumber++;
            CurrentRound = new Round(this, playersInOrder);
            Log.Debug("Created round {i} with starting player {player}", CurrentRoundNumber, playersInOrder[0].Name);
            RoundCreated?.Invoke(this, new(CurrentRoundNumber, CurrentRound));

            // Start the round
            CurrentRound.Start();

            var result = CurrentRound.Result!;
            Log.Information("Result of round {i}: \n{result}", CurrentRoundNumber, result);
            ApplyRoundResult(result);

            _finishedRounds.Add(CurrentRound);
            // Next player starts next round
            currentStartIdx = (currentStartIdx + 1) % 4;
        }

        CurrentRound = null;
        IsRunning = false;
        IsFinished = true;
        Log.Information("Game finished.");

        // Determine ranking of players based on points
        var ranking = Points.OrderBy(kv => kv.Value) // Sort by points ascending
            .Reverse() // Descending
            .Select(kv => kv.Key); // Select players
        GameFinished?.Invoke(this, new(ranking));
    }

    /// <summary>
    /// Adds a reservation factory to the game.
    /// An existing factories for the same reservation name will be overridden. 
    /// </summary>
    /// <param name="factory"></param>
    public void AddReservationFactory(ReservationFactory factory)
    {
        ReservationFactories[factory.ReservationName] = factory;
    }

    #endregion

    #region Private & Protected Methods

    /// <summary>
    /// Applies the given round results of the current round to the points table.
    /// </summary>
    /// <param name="result"></param>
    protected void ApplyRoundResult(RoundResult result)
    {
        List<KeyValuePair<Player, int>> pointChanges = new();

        var reParty = CurrentRound!.Description.ReParty;
        var contraParty = CurrentRound.Description.ContraParty;
        var isSolo = CurrentRound.Description.IsSolo;

        var winners = result.RePartyWon ? reParty : contraParty;
        var loosers = result.RePartyWon ? contraParty : reParty;

        // Determine point changes based on results of round
        Log.Debug("Write results of round to points table.");
        if (isSolo)
        {
            if (result.RePartyWon)
            {
                pointChanges.Add(new(winners[0], 3 * result.BasePoints));
                Log.Information("{player} won {points}.", winners[0].Name, 3 * result.BasePoints);
                foreach (var looser in loosers)
                {
                    pointChanges.Add(new(looser, -result.BasePoints));
                    Log.Information("{player} lost {points}.", looser.Name, result.BasePoints);
                }
            }
            else
            {
                pointChanges.Add(new(loosers[0], -3 * result.BasePoints));
                Log.Information("{player} lost {points}.", loosers[0].Name, 3 * result.BasePoints);
                foreach (var winner in winners)
                {
                    pointChanges.Add(new(winner, result.BasePoints));
                    Log.Information("{player} won {points}.", winner.Name, result.BasePoints);
                }
            }
        }
        else
        {
            foreach (var winner in winners)
            {
                pointChanges.Add(new(winner, result.BasePoints));
                Log.Information("{player} won {points}.", winner.Name, result.BasePoints);
            }
            foreach (var looser in loosers)
            {
                pointChanges.Add(new(looser, -result.BasePoints));
                Log.Information("{player} lost {points}.", looser.Name, result.BasePoints);
            }
        }

        // Apply changes
        pointChanges.ForEach(pc => _points[pc.Key] += pc.Value);
        // Invoke PointsUpdatedEvent
        PointsUpdated?.Invoke(this, new(pointChanges));
    }

    #endregion
}