using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib;

/// <summary>
/// An enumeration of Doko reservations.
/// </summary>
public enum ReservationType
{
    GiveHandsAgain,
    Armut,
    Hochzeit,
    //Fleshless,
    //ColorSolo,
    //BubenSolo,
    //DamenSolo
}

/// <summary>
/// Describes a reservation of a Doko round.
/// </summary>
public class Reservation
{
    #region Properties 

    /// <summary>
    /// The type of reservation.
    /// </summary>
    public ReservationType Type { get; init; }

    /// <summary>
    /// The player who has the reservation.
    /// </summary>
    public Player Player { get; init; }

    /// <summary>
    /// The name of the reservation.
    /// </summary>
    public string Name { get; init; }

    #endregion

    public Reservation(Player player, ReservationType type, string name = "")
    {
        Player = player;
        Type = type;
        Name = name;
    }
}
