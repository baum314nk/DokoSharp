using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Lib.Messaging;

/// <summary>
/// A reply message that contains the indentifier of a hand card of the player.
/// </summary>
public class ReplyHandCardMessage : Message
{
    [JsonPropertyName("card_id")]
    public string? CardIdentifier { get; set; }

    public ReplyHandCardMessage(string playerId, string cardId) : base("REPLY_HAND_CARD", playerId)
    {
        CardIdentifier = cardId;
    }
}
