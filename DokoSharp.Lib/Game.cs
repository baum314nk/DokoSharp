using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public IReadOnlyCollection<SpecialRule> SpecialRules { get; protected set; }

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
    public int MaximumRoundNumber { get; protected set; } = 0;

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

    /// <summary>
    /// Creates a new game with the given players.
    /// The players will sit in the order they were passed to this method.
    /// </summary>
    public Game(
        Tuple<string, IPlayerController>[] players, 
        IEnumerable<SpecialRule> specialRules)
    {
        if (players.Length != 4) throw new ArgumentException("Exactly 4 player names and associated controllers must be provided.", nameof(players));

        SpecialRules = specialRules.ToArray();
        _players = players.Select(p => new Player(p.Item2, this, p.Item1)).ToArray();
        _points = Players.ToDictionary(p => p, _ => 0);
        _finishedRounds = new();
    }

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

        MaximumRoundNumber = numOfRounds;
        IsRunning = true;
        Log.Information("Game started.");

        // Invoke special rules
        Log.Debug("Call OnGameStarted callback of special rules.");
        SpecialRules.ForEach(rule => rule.OnGameStarted?.Invoke(this));

        // Game loop
        int currentStartIdx = 0;
        while (CurrentRoundNumber != MaximumRoundNumber)
        {
            // Determine order of players for the round
            Player[] playersInOrder = new Player[4];
            Array.Copy(_players, currentStartIdx, playersInOrder, 0, 4 - currentStartIdx);
            Array.Copy(_players, 0, playersInOrder, 4 - currentStartIdx, currentStartIdx);

            // Create round and increase round number
            CurrentRoundNumber++;
            CurrentRound = new Round(this, playersInOrder);
            Log.Debug("Created round {i} with starting player {player}", CurrentRoundNumber, playersInOrder[0].Name);

            // Start the round
            CurrentRound.Start();

            var result = CurrentRound.Result!;
            Log.Information("Result of round {i}: \n{result}", CurrentRoundNumber, result);
            ApplyResult(result);

            _finishedRounds.Add(CurrentRound);
            // Next player starts next round
            currentStartIdx = (currentStartIdx + 1) % 4;
        }

        CurrentRound = null;
        IsRunning = false;
        IsFinished = true;
        Log.Information("Game finished.");
    }

    /// <summary>
    /// Applies the given round results to the points table.
    /// </summary>
    /// <param name="result"></param>
    public void ApplyResult(RoundResult result)
    {
        // Apply results of round
        Log.Debug("Write results of round to points table.");
        if (result.IsSolo)
        {
            if (result.RePartyWon)
            {
                _points[result.Winners[0]] += 3 * result.BasePoints;
                Log.Information("{player} won {points}.", result.Winners[0].Name, 3 * result.BasePoints);
                foreach (var looser in result.Loosers)
                {
                    _points[looser] -= result.BasePoints;
                    Log.Information("{player} lost {points}.", looser.Name, result.BasePoints);
                }
            }
            else
            {
                _points[result.Loosers[0]] -= 3 * result.BasePoints;
                Log.Information("{player} lost {points}.", result.Loosers[0].Name, 3 * result.BasePoints);
                foreach (var winner in result.Winners)
                {
                    _points[winner] += result.BasePoints;
                    Log.Information("{player} won {points}.", winner.Name, result.BasePoints);
                }
            }
        }
        else
        {
            foreach (var winner in result.Winners)
            {
                _points[winner] += result.BasePoints;
                Log.Information("{player} won {points}.", winner.Name, result.BasePoints);
            }
            foreach (var looser in result.Loosers)
            {
                _points[looser] -= result.BasePoints;
                Log.Information("{player} lost {points}.", looser.Name, result.BasePoints);
            }
        }


        //if (Points.Values.Sum() != 0)
        //{
        //    Log.Debug("ERROR");
        //}
    }

    #endregion
}
