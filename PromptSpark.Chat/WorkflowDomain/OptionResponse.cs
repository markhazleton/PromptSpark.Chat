using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class OptionResponse
{
    [JsonPropertyName("response")]
    public required string Response { get; set; }
}
