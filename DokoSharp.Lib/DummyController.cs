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

    public Card RequestCard(Player player)
    {
        return player.Cards[0];
    }

    public Card RequestPlaceCard(Player player, Trick trick)
    {
        foreach (var card in player.Cards)
        {
            if (player.Game.CurrentRound?.CurrentTrick?.ValidatePlacing(player, card) ?? true)
            {
                return card;
            }
        }

        throw new Exception();
    }

    public Reservation? RequestReservation(Player player)
    {
        return null;
    }

    public bool RequestYesNo(Player player, string requestText)
    {
        return false;
    }

    public void SignalReceivedCards(Player player, IEnumerable<Card> receivedCards)
    {

    }
}
