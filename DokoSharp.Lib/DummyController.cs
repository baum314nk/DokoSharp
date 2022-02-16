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

    public Card RequestHandCard(Player player)
    {
        foreach (var card in player.HandCards)
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
}
