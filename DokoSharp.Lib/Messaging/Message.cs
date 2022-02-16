using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Lib.Messaging;

/// <summary>
/// Describes a generic message for the Doko game.
/// </summary>
public abstract class Message
{
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("player_id")]
    public string? PlayerId { get; set; }

    public Message(string subject, string playerId)
    {
        Subject = subject;
        PlayerId = playerId;
    }
}