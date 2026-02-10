using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

/// <summary>
/// Custom JSON converter to handle flexible deserialization of string properties that may appear as numbers in JSON.
/// </summary>
public class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32().ToString(),
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Null => string.Empty,
            _ => throw new JsonException($"Unexpected token type '{reader.TokenType}' for string property.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}