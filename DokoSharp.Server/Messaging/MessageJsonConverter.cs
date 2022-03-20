using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Server.Messaging;

/// <summary>
/// A custom JSON converter that can handle <see cref="Message"/> and its subclasses.
/// </summary>
public class MessageJsonConverter : JsonConverter<Message>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Message).IsAssignableFrom(typeToConvert);
    }

    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Check for null values
        if (reader.TokenType == JsonTokenType.Null) return null;

        var readerClone = reader;

        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

        string? propertyName = reader.GetString();
        if (propertyName != "Subject") throw new JsonException();

        reader.Read();
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();

        string subject = reader.GetString()!;

        if (Message.SubjectTypes.TryGetValue(subject, out var subjectType))
        {
            // Copy options and remove this serializer
            JsonSerializerOptions newOptions = new(options);
            var converter = options.GetConverter(typeof(Message));
            newOptions.Converters.Remove(converter);

            var result = (Message)JsonSerializer.Deserialize(ref readerClone, subjectType, newOptions)!;
            reader = readerClone;
            return result;
        }

        throw new JsonException("Unknown message subject.");
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        // Copy options and remove this serializer
        JsonSerializerOptions newOptions = new(options);
        var converter = options.GetConverter(typeof(Message));
        newOptions.Converters.Remove(converter);

        JsonSerializer.Serialize(writer, value, value.GetType(), newOptions);
    }
}
