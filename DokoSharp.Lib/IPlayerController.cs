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
    void SignalReceivedCards(Player player, IEnumerable<Card> receivedCards, bool clearedOldCards);

    /// <summary>
    /// Signals the controller that the player dropped cards.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cards"></param>
    void SignalDroppedCards(Player player, IEnumerable<Card> droppedCards);

    /// <summary>
    /// Requests a card from the controller to place into a trick.
    /// </summary>
    /// <returns></returns>
    Card RequestPlaceCard(Player player, Trick trick);

    /// <summary>
    /// Requests cards from the controller.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Card> RequestCards(Player player, int amount, string requestText);

    /// <summary>
    /// Requests a Yes-No decision from the controller.
    /// </summary>
    /// <returns></returns>
    bool RequestYesNo(Player player, string requestText);

    /// <summary>
    /// Requests a color from the controller.
    /// </summary>
    /// <returns></returns>
    CardColor RequestColor(Player player, string requestText);

    /// <summary>
    /// Requests a reservation from the controller.
    /// </summary>
    /// <returns></returns>
    string? RequestReservation(Player player, IEnumerable<string> possibilities);
}
