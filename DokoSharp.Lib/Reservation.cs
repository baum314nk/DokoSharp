using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib;

/// <summary>
/// A factory for reservation objects.
/// </summary>
public class ReservationFactory
{
    #region Fields

    private readonly Predicate<IEnumerable<Card>> _predicate;
    private readonly Func<Player, string, int, Reservation> _createFunc;

    #endregion

    /// <summary>
    /// The name of the reservation the factory can produce.
    /// </summary>
    public string ReservationName { get; init; }

    /// <summary>
    /// The rank of the reservation the factory can produce.
    /// </summary>
    public int ReservationRank { get; init; }

    /// <summary>
    /// Creates a new reservation factory.
    /// </summary>
    /// <param name="reservationName"></param>
    /// <param name="predicate"></param>
    /// <param name="createFunc"></param>
    public ReservationFactory(string reservationName, int reservationRank, Predicate<IEnumerable<Card>> predicate, Func<Player, string, int, Reservation>? createFunc = null)
    {
        ReservationName = reservationName;
        ReservationRank = reservationRank;
        _predicate = predicate;
        _createFunc = createFunc ?? ((player, name, rank) => new Reservation(player, name, rank));
    }

    /// <summary>
    /// Returns true if the given hand cards allow to have reservation created by the factory.
    /// </summary>
    /// <param name="handCards"></param>
    /// <returns></returns>
    public bool CheckPredicate(IEnumerable<Card> handCards) => _predicate(handCards);

    /// <summary>
    /// Creates a reservation for the given player.
    /// </summary>
    /// <param name="handCards"></param>
    /// <returns></returns>
    public Reservation Create(Player player) => _createFunc(player, ReservationName, ReservationRank);
}

/// <summary>
/// Describes a reservation of a Doko round.
/// </summary>
public class Reservation
{
    #region Properties

    /// <summary>
    /// The player who has the reservation.
    /// </summary>
    public Player Player { get; init; }

    /// <summary>
    /// The name of the reservation.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The rank of the reservation.
    /// </summary>
    public int Rank { get; init; }

    #endregion

    public Reservation(Player player, string name, int rank)
    {
        Player = player;
        Name = name;
        Rank = rank;
    }
}

/// <summary>
/// Describes a reservation that depends on a card color.
/// </summary>
public class ColorReservation : Reservation
{
    #region Properties

    /// <summary>
    /// The color of the reservation.
    /// </summary>
    public CardColor Color { get; init; }

    #endregion

    public ColorReservation(Player player, string name, int rank, CardColor color) : base(player, name, rank + (int)color)
    {
        Color = color;
    }
}
