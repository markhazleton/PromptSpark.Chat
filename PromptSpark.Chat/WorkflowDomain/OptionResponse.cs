using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class OptionResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; }
}
