using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Lib.Messaging;

/// <summary>
/// A request message that expects the player to reply with a card valid for the trick.
/// </summary>
public class RequestPlaceCardMessage : Message
{
    public const string SUBJECT = "REQUEST_PlaceCard";

    public override string Subject => SUBJECT;

    public IList<string>? TrickCards { get; set; }
}

/// <summary>
/// A request message that expects the player to select a card.
/// </summary>
public class RequestCardMessage : Message
{
    public const string SUBJECT = "REQUEST_Card";

    public override string Subject => SUBJECT;
}

/// <summary>
/// A reply message that contains the indentifier of the selected card of the player.
/// </summary>
public class ReplyCardMessage : Message
{
    public const string SUBJECT = "REPLY_Card";

    public override string Subject => SUBJECT;

    public string? CardIdentifier { get; set; }
}