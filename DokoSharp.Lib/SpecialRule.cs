using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib;

/// <summary>
/// Describes a special rule of a Doko game.
/// </summary>
public class SpecialRule : IIdentifiable
{
    #region Constants

    public static readonly SpecialRule KarlchenRule = new(
        "Karlchen",
        onTrickFinished: (trick) =>
        {
            var round = trick.Round;

            // Applies only to last trick
            if (round.CurrentTrickNumber != 12) return;

            var winner = trick.Winner!;
            var winnerIdx = trick.PlayersInOrder.IndexOf(winner);
            var winnerCard = trick.PlacedCards[winnerIdx];

            // Check if winner placed a Eichel Unter
            if (winnerCard.Base == CardBase.Existing[CardColor.Eichel]["U"])
            {
                string name = "Karlchen";
                Log.Information("Player {player} got an additional point because of a {desc}.", winner.Name, name);


                // Add bonus point to winner party
                if (round.Description.ReParty.Contains(winner))
                {
                    round.Description.ReAdditionalPoints.Add(name);
                }
                else
                {
                    round.Description.ContraAdditionalPoints.Add(name);
                }
            }
        });

    public static readonly SpecialRule DoppelkopfRule = new(
        "Doppelkopf",
        onTrickFinished: (trick) =>
        {
            var round = trick.Round;

            var winner = trick.Winner!;
            var winnerIdx = trick.PlayersInOrder.IndexOf(winner);

            // Check if trick contains 4 full cards
            if (trick.Value >= 40)
            {
                string name = "Doppelkopf";
                Log.Information("Player {player} got an additional point because of a {desc}.", winner.Name, name);

                // Add bonus point to winner party
                if (round.Description.ReParty.Contains(winner))
                {
                    round.Description.ReAdditionalPoints.Add(name);
                }
                else
                {
                    round.Description.ContraAdditionalPoints.Add(name);
                }
            }
        });

    public static readonly SpecialRule Herz10Rule = new(
        "Herz 10 Highest Trump",
        onRoundStarted: (round) =>
        {
            // Make Herz 10 highest trump
            round.Description.TrumpRanking.Add(CardBase.Existing[CardColor.Herz]["10"]);
            Log.Information("Herz 10 is highest trump.");
        });

    public static readonly SpecialRule SchweinchenRule = new(
        "Schweinchen",
        onApplyRegistrations: (round) =>
        {
            Player? affectedPlayer = null;

            // Find affected player
            CardBase schellAss = CardBase.Existing[CardColor.Schell]["A"];
            foreach (Player player in round.PlayersInOrder)
            {
                int karoAssCount = player.Cards!.Select(c => c.Base == schellAss ? 1 : 0).Sum();

                if (karoAssCount == 2)
                {
                    affectedPlayer = player;
                    break;
                }
            }
            if (affectedPlayer is null) return;

            // Ask player if he wants to announce his Schweinchen
            bool announce = affectedPlayer.DecideYesNo("Do you want to announce your Schweinchen?");

            if (announce)
            {
                // Make karo ass the highest ranking card
                round.Description.TrumpRanking.Remove(schellAss);
                round.Description.TrumpRanking.Add(schellAss);
                Log.Information("Player {player} announced a Schweinchen. Karo Ass is the highest trump.", affectedPlayer.Name);
            }
        });

    public static readonly SpecialRule ArmutRule = new(
        "Armut",
        onReservationsPerformed: (round) =>
        {
            if (round.Description.ActiveReservation?.Type != ReservationType.Armut) return;

            Player armutPlayer = round.Description.ActiveReservation.Player;

            // Find party member of Armut
            Player? memberPlayer = null;
            foreach (Player player in round.PlayersInOrder)
            {
                if (player == armutPlayer) continue;

                // Ask player if he wants to play the Armut
                if (player.DecideYesNo("Do you want to play the Armut?"))
                {
                    memberPlayer = player;
                    Log.Information("Player {player} accepted the Armut of player {armutPlayer}.", memberPlayer.Name, armutPlayer.Name);
                    break;
                }
            }
            if (memberPlayer is null)
            {
                round.Description.ActiveReservation = null;
                Log.Information("No player accepted the Armut of player {armutPlayer}.", armutPlayer.Name);
                return;
            }

            // Armut party exchanges 2 cards
            var armutCards = armutPlayer.DropCards(2);
            memberPlayer.ReceiveCards(armutCards, clearOldCards: false);
            var memberCards = memberPlayer.DropCards(2);
            armutPlayer.ReceiveCards(memberCards, clearOldCards: false);
            Log.Information("Armut party exchanged 2 cards.");

            // Armut party becomes Re
            round.Description.ReParty.Clear();
            round.Description.ReParty.Add(armutPlayer);
            round.Description.ReParty.Add(memberPlayer);

            // Other players become Contra
            round.Description.ContraParty.Clear();
            foreach (Player player in round.PlayersInOrder.Where(p => !round.Description.ReParty.Contains(p)))
            {
                round.Description.ContraParty.Add(player);
            }
        });

    public static readonly SpecialRule HochzeitRule = new(
        "Hochzeit",
        onTrickFinished: (trick) =>
        {
            var round = trick.Round;

            if (round.Description.ActiveReservation?.Type != ReservationType.Hochzeit ||
                round.Description.ReParty.Count > 1 ||
                round.CurrentTrickNumber > 3) return;

            // Stop early if winner is Hochzeit player
            if (trick.Winner == round.Description.ReParty[0]) return;

            // Winner of trick is in Re party
            round.Description.ReParty.Add(trick.Winner!);
            round.Description.ContraParty.Remove(trick.Winner!);
        });

    #endregion

    /// <summary>
    /// Returns the default special rules of a Doko game.
    /// </summary>
    public static IEnumerable<SpecialRule> GetDefaults()
    {
        return new[]
        {
            Herz10Rule,
            ArmutRule,
            HochzeitRule,
            KarlchenRule,
            DoppelkopfRule,
            SchweinchenRule,
        };
    }

    #region Properties

    /// <summary>
    /// The name of the rule.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The identifier of the rule.
    /// Corresponds to the name
    /// </summary>
    public string Identifier => Name;

    /// <summary>
    /// The action that gets performed when the game starts.
    /// </summary>
    public Action<Game>? OnGameStarted { get; init; }

    /// <summary>
    /// The action that gets performed when the round starts.
    /// </summary>
    public Action<Round>? OnRoundStarted { get; init; }

    /// <summary>
    /// The action that gets performed when the hand cards where given.
    /// </summary>
    public Action<Round>? OnHandsGiven { get; init; }

    /// <summary>
    /// The action that gets performed when the reservations where performed.
    /// </summary>
    public Action<Round>? OnReservationsPerformed { get; init; }

    /// <summary>
    /// The action that gets performed when the registrations are applied.
    /// </summary>
    public Action<Round>? OnApplyRegistrations { get; init; }

    /// <summary>
    /// The action that gets performed when a trick has ended.
    /// </summary>
    public Action<Trick>? OnTrickFinished { get; init; }

    #endregion

    public SpecialRule(
        string name,
        Action<Game>? onGameStarted = null, 
        Action<Round>? onRoundStarted = null,
        Action<Round>? onHandsGiven = null,
        Action<Round>? onReservationsPerformed = null,
        Action<Round>? onApplyRegistrations = null,
        Action<Trick>? onTrickFinished = null
        )
    {
        Name = name;
        OnGameStarted = onGameStarted;
        OnRoundStarted = onRoundStarted;
        OnHandsGiven = onHandsGiven;
        OnReservationsPerformed = onReservationsPerformed;
        OnApplyRegistrations = onApplyRegistrations;
        OnTrickFinished = onTrickFinished;
    }
}
