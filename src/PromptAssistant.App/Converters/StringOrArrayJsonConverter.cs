using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptAssistant.App.Converters;

/// <summary>
/// JSON converter that tolerates a string field arriving as either:
///   - a JSON string (used as-is), or
///   - a JSON array of strings (joined with '\n').
///
/// LLMs occasionally interpret "bullet-point lines separated by newlines" as "array of strings"
/// when generating prompt templates. This converter unifies both shapes at the parse boundary
/// so the rest of the app sees a single string regardless of which format the model emitted.
/// </summary>
public sealed class StringOrArrayJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? "";
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var items = new List<string>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    items.Add(reader.GetString() ?? "");
                }
                else if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    reader.Skip();
                }
            }
            return string.Join("\n", items.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return "";
        }

        // Defensive: skip unknown tokens (numbers, booleans, objects) and return empty.
        reader.Skip();
        return "";
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
