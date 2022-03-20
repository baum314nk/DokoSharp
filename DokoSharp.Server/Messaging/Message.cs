using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Server.Messaging;

/// <summary>
/// Describes a generic message for the Doko game.
/// </summary>
public abstract class Message
{
    public static readonly IReadOnlyDictionary<string, Type> SubjectTypes;

    static Message()
    {
        var subjectTypes = new Dictionary<string, Type>();

        var assembly = Assembly.GetExecutingAssembly();
        var msgTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Message)));
        foreach (var msgType in msgTypes)
        {
            var subject = (string)msgType.GetField("SUBJECT")!.GetValue(null)!;
            subjectTypes[subject] = msgType;
        }

        SubjectTypes = subjectTypes;
    }

    [JsonInclude]
    public abstract string? Subject { get; }

    public Message()
    {

    }
}