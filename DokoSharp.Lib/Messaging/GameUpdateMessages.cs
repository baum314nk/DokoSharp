using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Lib.Messaging;

public class GameCreatedMessage : Message
{
    public const string SUBJECT = "UPDATE_GameCreated";

    public override string Subject => SUBJECT;

    public IList<string>? Players { get; set; }

    public IList<string>? SpecialRules { get; set; }
}

public class GameStartedMessage : Message
{
    public const string SUBJECT = "UPDATE_GameStarted";

    public override string Subject => SUBJECT;

    public int NumberOfRounds { get; set; }
}

public class PointsUpdatedMessage : Message
{
    public const string SUBJECT = "UPDATE_PointsUpdated";

    public override string Subject => SUBJECT;

    public IDictionary<string, int>? PointChanges { get; set; }
}

public class GameFinishedMessage : Message
{
    public const string SUBJECT = "UPDATE_GameFinished";

    public override string Subject => SUBJECT;

    public IList<string>? Ranking { get; set; }
}

public class RoundCreatedMessage : Message
{
    public const string SUBJECT = "UPDATE_RoundCreated";

    public override string Subject => SUBJECT;

    public int RoundNumber { get; set; }

    public IList<string>? PlayerOrder { get; set; }
}

public class RoundStartedMessage : Message
{
    public const string SUBJECT =  "UPDATE_RoundStarted";

    public override string Subject => SUBJECT;

    public IList<string>? TrumpRanking { get; set; }
}

public class ReservationsPerformedMessage : Message
{
    public const string SUBJECT = "UPDATE_ReservationsPerformed";

    public override string Subject => SUBJECT;

    public string? ActiveReservation { get; set; }
}

public class RegistrationsAppliedMessage : Message
{
    public const string SUBJECT = "UPDATE_RegistrationsApplied";

    public override string Subject => SUBJECT;

    public IList<string>? TrumpRanking { get; set; }
}

public class RoundFinishedMessage : Message
{
    public const string SUBJECT =  "UPDATE_RoundFinished";

    public override string Subject => SUBJECT;

    public bool? RePartyWon { get; set; }

    public IList<string>? ReParty { get; set; }

    public IList<string>? ReAdditionalPoints { get; set; }

    public int? ReValue { get; set; }

    public IList<string>? ContraParty { get; set; }

    public IList<string>? ContraAdditionalPoints { get; set; }

    public int? ContraValue { get; set; }

    public bool? IsSolo { get; init; }

    public int BasePoints { get; init; }
}

public class TrickCreatedMessage : Message
{
    public const string SUBJECT = "UPDATE_TrickCreated";

    public override string Subject => SUBJECT;

    public int TrickNumber { get; set; }

    public IList<string>? PlayerOrder { get; set; }
}

public class CardPlacedMessage : Message
{
    public const string SUBJECT = "UPDATE_CardPlaced";

    public override string Subject => SUBJECT;

    public string? Player { get; set; }

    public string? PlacedCard { get; set; }
}

public class TrickFinishedMessage : Message
{
    public const string SUBJECT = "UPDATE_TrickFinished";

    public override string Subject => SUBJECT;

    public string? Winner { get; set; }

    public int? Value { get; set; }
}

public class CardsReceivedMessage : Message
{
    public const string SUBJECT = "UPDATE_CardsReceived";

    public override string Subject => SUBJECT;

    public IList<string>? ReceivedCards { get; set; }

    public bool ClearedOldCards { get; set; }
}

public class CardsDroppedMessage : Message
{
    public const string SUBJECT = "UPDATE_CardsDropped";

    public override string Subject => SUBJECT;

    public IList<string>? DroppedCards { get; set; }
}