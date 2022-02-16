using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib;

/// <summary>
/// An interface for a player controller.
/// </summary>
public interface IPlayerController
{
    /// <summary>
    /// Requests a hand card from the controller.
    /// </summary>
    /// <returns></returns>
    Card RequestHandCard(Player player);

    /// <summary>
    /// Requests a Yes-No decision from the controller.
    /// </summary>
    /// <returns></returns>
    bool RequestYesNo(Player player, string requestText);

    /// <summary>
    /// Requests a reservation from the controller.
    /// </summary>
    /// <returns></returns>
    Reservation? RequestReservation(Player player);
}
