using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class Answer
{

    [JsonPropertyName("nextNode")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string NextNode { get; set; }
    [JsonPropertyName("response")]
    public string Response { get; set; }
    [JsonPropertyName("system")]
    public string SystemPrompt { get; set; } = "You are a chat agent";
}
