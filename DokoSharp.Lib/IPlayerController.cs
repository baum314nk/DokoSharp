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
    /// Signals the controller that the player received cards.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cards"></param>
    void SignalReceivedCards(Player player, IEnumerable<Card> receivedCards);

    /// <summary>
    /// Requests a card from the controller to place into a trick.
    /// </summary>
    /// <returns></returns>
    Card RequestPlaceCard(Player player, Trick trick);

    /// <summary>
    /// Requests a card from the controller.
    /// </summary>
    /// <returns></returns>
    Card RequestCard(Player player);

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
