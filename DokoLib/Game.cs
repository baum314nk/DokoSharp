using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoLib;

/// <summary>
/// Describes a game of Doko.
/// </summary>
public class Game
{
    #region Fields

    protected Dictionary<Player, int> _points;
    protected Player[] _players;

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
    public int CurrrentRoundNumber { get; protected set; } = 0;

    /// <summary>
    /// The number of rounds that should be played.
    /// </summary>
    public int MaximumRoundNumber { get; protected set; }

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
    public Game(string playerName1, string playerName2, string playerName3, string playerName4, IEnumerable<SpecialRule> specialRules, int numOfRounds = 4)
    {
        SpecialRules = specialRules.ToArray();
        _players = new Player[] {
            new(this, playerName1),
            new(this, playerName2),
            new(this, playerName3),
            new(this, playerName4),
        };
        _points = Players.ToDictionary(p => p, _ => 0);
        MaximumRoundNumber = numOfRounds;
    }

    /// <summary>
    /// Starts the game.
    /// Does nothing if it is already running.
    /// </summary>
    public void Start()
    {
        if (IsRunning || IsFinished) return;

        IsRunning = true;
        // Invoke special rules
        SpecialRules.ForEach(rule => rule.OnGameStarted?.Invoke(this));

        // Game loop
        int currentStartIdx = 0;
        while (CurrrentRoundNumber != MaximumRoundNumber)
        {
            CurrrentRoundNumber++;

            // Determine order of players for the round
            Player[] playersInOrder = new Player[4];
            Array.Copy(_players, currentStartIdx, playersInOrder, 0, 4 - currentStartIdx);
            Array.Copy(_players, 0, playersInOrder, 4 - currentStartIdx, currentStartIdx);

            // Create round and increase round number
            CurrrentRoundNumber++;
            CurrentRound = new Round(this, playersInOrder);

            // Start the round
            CurrentRound.Start();

            // Apply results of round
            var result = CurrentRound.Result!;
            if (result.IsSolo)
            {
                _points[result.Winners[0]] += 3 * result.BasePoints;
                foreach (var looser in result.Losers) _points[looser] -= result.BasePoints;
            } else
            {
                foreach (var winner in result.Winners) _points[winner] += result.BasePoints;
                foreach (var looser in result.Losers) _points[looser] -= result.BasePoints;
            }

            // Next player starts next round
            currentStartIdx = (currentStartIdx + 1) % 4;
        }

        CurrentRound = null;
        IsRunning = false;
        IsFinished = true;
    }
}
