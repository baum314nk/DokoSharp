using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib.Rulings;

/// <summary>
/// Describes a special rule of a Doko game.
/// </summary>
public class Rule : IIdentifiable
{
    #region Constants

    public static readonly Rule OberSolo = new(
        "Vorbehalt: Ober Solo",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Ober Solo",
                10,
                (handCards) => true
            ));
        },
        onReservationsPerformed: (round, reservation) =>
        {
            if (reservation?.Name != "Ober Solo") return;

            Player soloPlayer = reservation.Player;

            // Solo player becomes Re
            round.Description.ReParty.Clear();
            round.Description.ReParty.Add(soloPlayer);

            // Other players become Contra
            round.Description.ContraParty.Clear();
            foreach (Player player in round.PlayersInOrder)
            {
                if (player == soloPlayer) continue;
                round.Description.ContraParty.Add(player);
            }

            // Only Ober are trump
            round.Description.TrumpRanking = Enum.GetValues<CardColor>().Select(color => CardBase.Existing[color]["O"]).ToList();
        });

    public static readonly Rule UnterSolo = new(
        "Vorbehalt: Unter Solo",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Unter Solo",
                9,
                (handCards) => true
            ));
        },
        onReservationsPerformed: (round, reservation) =>
        {
            if (reservation?.Name != "Unter Solo") return;

            Player soloPlayer = reservation.Player;

            // Solo player becomes Re
            round.Description.ReParty.Clear();
            round.Description.ReParty.Add(soloPlayer);

            // Other players become Contra
            round.Description.ContraParty.Clear();
            foreach (Player player in round.PlayersInOrder)
            {
                if (player == soloPlayer) continue;
                round.Description.ContraParty.Add(player);
            }

            // Only Unter are trump
            round.Description.TrumpRanking = Enum.GetValues<CardColor>().Select(color => CardBase.Existing[color]["U"]).ToList();
        });

    public static readonly Rule FarbSolo = new(
        "Vorbehalt: Farb Solo",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Farb Solo",
                4,
                (handCards) => true,
                (player, name, rank) =>
                {
                    var color = player.DecideColor("Please select the color of the Farb Solo you want to play.");
                    return new ColorReservation(player, name, rank, color);
                }
            ));
        },
        onReservationsPerformed: (round, reservation) =>
        {
            if (reservation?.Name != "Farb Solo") return;

            Player soloPlayer = reservation.Player;
            CardColor color = ((ColorReservation)reservation!).Color;

            // Solo player becomes Re
            round.Description.ReParty.Clear();
            round.Description.ReParty.Add(soloPlayer);

            // Other players become Contra
            round.Description.ContraParty.Clear();
            foreach (Player player in round.PlayersInOrder)
            {
                if (player == soloPlayer) continue;
                round.Description.ContraParty.Add(player);
            }

            // Only Unter are trump
            round.Description.TrumpRanking = Enum.GetValues<CardColor>().Select(color => CardBase.Existing[color]["U"]).ToList();
        });

    public static readonly Rule Fleischlos = new(
        "Vorbehalt: Fleischlos",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Fleischlos",
                3,
                (handCards) => true
            ));
        },
        onReservationsPerformed: (round, reservation) =>
        {
            if (reservation?.Name != "Fleischlos") return;

            Player soloPlayer = reservation.Player;

            // Solo player becomes Re
            round.Description.ReParty.Clear();
            round.Description.ReParty.Add(soloPlayer);

            // Other players become Contra
            round.Description.ContraParty.Clear();
            foreach (Player player in round.PlayersInOrder)
            {
                if (player == soloPlayer) continue;
                round.Description.ContraParty.Add(player);
            }

            // No trump exists
            round.Description.TrumpRanking.Clear();
        });

    public static readonly Rule HochzeitRule = new(
        "Vorbehalt: Hochzeit",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Hochzeit",
                2,
                (handCards) => handCards.Where(c => c.Base.Identifier == "Eichel_O").Count() == 2
            ));
        },
        onTrickFinished: (trick) =>
        {
            var round = trick.Round;

            if (round.Description.ActiveReservation?.Name != "Hochzeit" ||
                round.Description.ReParty.Count > 1 ||
                round.CurrentTrickNumber > 3) return;

            // Stop early if winner is Hochzeit player
            if (trick.Winner == round.Description.ReParty[0]) return;

            // Winner of trick is in Re party
            round.Description.ReParty.Add(trick.Winner!);
            round.Description.ContraParty.Remove(trick.Winner!);
        });

    public static readonly Rule ArmutRule = new(
        "Vorbehalt: Armut",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Armut",
                1,
                (handCards) => handCards.Where(c => c.IsTrump).Count() <= 3
            ));
        },
        onReservationsPerformed: (round, reservation) =>
        {
            if (reservation?.Name != "Armut") return;

            Player armutPlayer = reservation.Player;

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
            var armutCards = armutPlayer.DropCards("Please select the 2 cards you want to give your partner.", 2);
            memberPlayer.ReceiveCards(armutCards);
            var memberCards = memberPlayer.DropCards("Please select the 2 cards you want to give your partner.", 2);
            armutPlayer.ReceiveCards(memberCards);
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

    public static readonly Rule Einmischen5x9 = new(
        "Vorbehalt: 5 mal 9 Einmischen",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Einmischen",
                0,
                (handCards) => handCards.Where(c => c.Base.Symbol == "9").Count() >= 5
            ));
        });

    public static readonly Rule Einmischen7x9 = new(
        "Vorbehalt: 7 mal Volle Einmischen",
        onGameStarted: (game) =>
        {
            game.AddReservationFactory(new(
                "Einmischen",
                0,
                (handCards) => handCards.Where(c => c.Base.Value >= 10).Count() >= 7
            ));
        });

    public static readonly Rule KarlchenRule = new(
        "Extrapunkt: Karlchen",
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

    public static readonly Rule DoppelkopfRule = new(
        "Extrapunkt: Doppelkopf",
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

    public static readonly Rule Herz10Rule = new(
        "Herz 10 Highest Trump",
        onRoundStarted: (round) =>
        {
            // Make Herz 10 highest trump
            round.Description.TrumpRanking.Add(CardBase.Existing[CardColor.Herz]["10"]);
            Log.Information("Herz 10 is highest trump.");
        });

    public static readonly Rule SchweinchenRule = new(
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

    #endregion

    /// <summary>
    /// Returns the default special rules of a Doko game.
    /// </summary>
    public static IEnumerable<Rule> GetDefaults()
    {
        return new[]
        {
            Herz10Rule,
            KarlchenRule,
            DoppelkopfRule,
            SchweinchenRule,
            Einmischen5x9,
            Einmischen7x9,
            ArmutRule,
            HochzeitRule,
            Fleischlos,
            FarbSolo,
            UnterSolo,
            OberSolo,
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
    /// Receives the current round and the active reservation as arguments.
    /// </summary>
    public Action<Round, Reservation?>? OnReservationsPerformed { get; init; }

    /// <summary>
    /// The action that gets performed when the registrations are applied.
    /// </summary>
    public Action<Round>? OnApplyRegistrations { get; init; }

    /// <summary>
    /// The action that gets performed when a trick has ended.
    /// </summary>
    public Action<Trick>? OnTrickFinished { get; init; }

    #endregion

    public Rule(
        string name,
        Action<Game>? onGameStarted = null,
        Action<Round>? onRoundStarted = null,
        Action<Round>? onHandsGiven = null,
        Action<Round, Reservation?>? onReservationsPerformed = null,
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
