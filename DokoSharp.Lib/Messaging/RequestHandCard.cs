using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib.Messaging;

/// <summary>
/// A request message that expects a hand card of the player as result.
/// </summary>
public class RequestHandCardMessage : Message
{
    public RequestHandCardMessage(string playerId) : base("REQUEST_HAND_CARD", playerId)
    {

    }
}