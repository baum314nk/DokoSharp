using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoLib;

/// <summary>
/// Describes a special rule of a Doko game.
/// </summary>
public class SpecialRule
{
    #region Constants

    public static readonly SpecialRule KarlchenRule = new(
        onTrickEnded: (trick) =>
        {
            var round = trick.Round;

            // Applies only to last trick
            if (round.CurrentTrickNumber != 12) return;

            var winner = trick.Winner!;
            var winnerIdx = trick.PlayersInOrder.IndexOf(winner);
            var winnerCard = trick.PlacedCards[winnerIdx];

            // Check if winner placed a Kreuz Jack
            if (winnerCard.Base == CardBase.Existing[CardColor.Kreuz]["B"])
            {
                string description = "Karlchen";

                // Add bonus point to winner party
                if (round.Description.ReParty.Contains(winner))
                {
                    round.Description.ReAdditionalPoints.Add(description);
                }
                else
                {
                    round.Description.ContraAdditionalPoints.Add(description);
                }
            }
        });

    public static readonly SpecialRule DoppelkopfRule = new(
        onTrickEnded: (trick) =>
        {
            var round = trick.Round;

            var winner = trick.Winner!;
            var winnerIdx = trick.PlayersInOrder.IndexOf(winner);

            // Check if trick contains 4 full cards
            if (trick.Value >= 40)
            {
                string description = "Doppelkopf";

                // Add bonus point to winner party
                if (round.Description.ReParty.Contains(winner))
                {
                    round.Description.ReAdditionalPoints.Add(description);
                }
                else
                {
                    round.Description.ContraAdditionalPoints.Add(description);
                }
            }
        });

    public static readonly SpecialRule Herz10Rule = new(
        onRoundStarted: (round) =>
        {
            // Make Herz 10 highest trump
            round.Description.TrumpRanking.Add(CardBase.Existing[CardColor.Herz]["10"]);
        });

    public static readonly SpecialRule SchweinchenRule = new(
        onReservationsResolved: (round) =>
        {
            Player? affectedPlayer = null;

            // Find affected player
            CardBase karoAss = CardBase.Existing[CardColor.Karo]["A"];
            foreach (Player player in round.PlayersInOrder)
            {
                int karoAssCount = player.HandCards!.Select(c => c.Base == karoAss ? 1 : 0).Sum();

                if (karoAssCount == 2)
                {
                    affectedPlayer = player;
                    break;
                }
            }
            if (affectedPlayer is null) return;

            // Ask player if he wants to announce his Schweinchen
            bool announce = affectedPlayer.RequestYesNo("Do you want to announce your Schweinchen?");

            if (announce)
            {
                // Make karo ass the highest ranking card
                round.Description.TrumpRanking.Remove(karoAss);
                round.Description.TrumpRanking.Add(karoAss);
            }
        });

    public static readonly SpecialRule ArmutRule = new(
        onReservationsResolved: (round) =>
        {
            if (round.Description.ActiveReservation?.Type != ReservationType.Armut) return;

            Player armutPlayer = round.Description.ActiveReservation.Player;

            // Find party member of Armut
            Player? memberPlayer = null;
            foreach (Player player in round.PlayersInOrder)
            {
                if (player == armutPlayer) continue;

                // Ask player if he wants to play the Armut
                if (player.RequestYesNo("Do you want to play the Armut?"))
                {
                    memberPlayer = player;
                    break;
                }
            }
            if (memberPlayer is null)
            {
                round.Description.ActiveReservation = null;
                return;
            }

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
        onTrickEnded: (trick) =>
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
    /// The action that gets performed when a trick has ended.
    /// </summary>
    public Action<Trick>? OnTrickEnded { get; init; }

    #endregion

    public SpecialRule(
        Action<Game>? onGameStarted = null, 
        Action<Round>? onRoundStarted = null,
        Action<Round>? onHandsGiven = null,
        Action<Round>? onReservationsResolved = null,
        Action<Trick>? onTrickEnded = null
        )
    {
        OnGameStarted = onGameStarted;
        OnRoundStarted = onRoundStarted;
        OnHandsGiven = onHandsGiven;
        OnReservationsPerformed = onReservationsResolved;
        OnTrickEnded = onTrickEnded;
    }
}
