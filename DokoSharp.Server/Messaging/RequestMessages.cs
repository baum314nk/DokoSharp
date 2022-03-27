using DokoSharp.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Server.Messaging;

public class ReplyReadyMessage {
    
}

/// <summary>
/// A request message that expects the player to reply with a card valid for the trick.
/// </summary>
public class RequestPlaceCardMessage : Message
{
    public const string SUBJECT = "REQUEST_PlaceCard";

    public override string Subject => SUBJECT;

    public IList<string>? TrickCards { get; set; }

    public bool CanMakeAnnouncement { get; set; }
}
/// <summary>
/// A reply message that contains the indentifiers of the selected card of the player
/// and optionally an announcement.
/// </summary>
public class ReplyPlaceCardMessage : Message
{
    public const string SUBJECT = "REPLY_PlaceCard";

    public override string Subject => SUBJECT;

    public IList<string>? CardIdentifiers { get; set; }

    public Announcement Announcement { get; set; }
}

/// <summary>
/// A request message that expects the player to select one or more cards.
/// </summary>
public class RequestCardsMessage : Message
{
    public const string SUBJECT = "REQUEST_Card";

    public override string Subject => SUBJECT;

    public int Amount { get; set; }

    public string? RequestText { get; set; }
}
/// <summary>
/// A reply message that contains the indentifiers of the selected cards of the player.
/// </summary>
public class ReplyCardsMessage : Message
{
    public const string SUBJECT = "REPLY_Card";

    public override string Subject => SUBJECT;

    public IList<string>? CardIdentifiers { get; set; }
}


/// <summary>
/// A request message that expects the player to select a color.
/// </summary>
public class RequestColorMessage : Message
{
    public const string SUBJECT = "REQUEST_Color";

    public override string Subject => SUBJECT;

    public string? RequestText { get; set; }
}
/// <summary>
/// A reply message that contains the color of the selected color of the player.
/// </summary>
public class ReplyColorMessage : Message
{
    public const string SUBJECT = "REPLY_Color";

    public override string Subject => SUBJECT;

    public CardColor Color { get; set; }
}


/// <summary>
/// A request message that expects the player to make a yes-no-decision.
/// </summary>
public class RequestYesNoMessage : Message
{
    public const string SUBJECT = "REQUEST_YesNo";

    public override string Subject => SUBJECT;

    public string? RequestText { get; set; }
}
/// <summary>
/// A reply message that contains the color of the selected color of the player.
/// </summary>
public class ReplyYesNoMessage : Message
{
    public const string SUBJECT = "REPLY_YesNo";

    public override string Subject => SUBJECT;

    public bool IsYes { get; set; }
}


/// <summary>
/// A request message that expects the player to select a reservation from a
/// set of choices or none.
/// </summary>
public class RequestReservationMessage : Message
{
    public const string SUBJECT = "REQUEST_Reservation";

    public override string Subject => SUBJECT;

    public IList<string>? Possibilities { get; set; }
}
/// <summary>
/// A reply message that contains the selected reservation name if any of the player.
/// </summary>
public class ReplyReservationMessage : Message
{
    public const string SUBJECT = "REPLY_Reservation";

    public override string Subject => SUBJECT;

    public string? ReservationName { get; set; }
}
