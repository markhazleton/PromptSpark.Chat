using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

/// <summary>
/// Custom JSON converter to handle flexible deserialization of string properties that may appear as numbers in JSON.
/// </summary>
public class FlexibleStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32().ToString();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        throw new JsonException("Unexpected token type for Id property.");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}