using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib;

/// <summary>
/// A dummy player controller.
/// </summary>
public class DummyController : IPlayerController
{
    public static readonly DummyController Instance = new();

    public Tuple<Card, Announcement> RequestPlaceCard(Player player, Trick trick, bool canMakeAnnouncement)
    {
        foreach (var card in player.Cards)
        {
            if (player.Game.CurrentRound?.CurrentTrick?.ValidatePlacing(player, card) ?? true)
            {
                return new(card, Announcement.None);
            }
        }

        throw new Exception();
    }

    public string? RequestReservation(Player player, IEnumerable<string> possibilities)
    {
        return null;
    }

    public bool RequestYesNo(Player player, string requestText)
    {
        return false;
    }

    public void SignalReceivedCards(Player player, IEnumerable<Card> receivedCards, bool clearedOldCards)
    {

    }

    public void SignalDroppedCards(Player player, IEnumerable<Card> droppedCards)
    {

    }

    public IEnumerable<Card> RequestCards(Player player, int amount, string requestText)
    {
        return player.Cards.Take(amount);
    }

    public CardColor RequestColor(Player player, string requestText)
    {
        return CardColor.Schell;
    }
}
