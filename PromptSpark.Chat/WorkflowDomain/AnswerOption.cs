using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class AnswerOption
{

    [JsonPropertyName("link")]
    public required string Link { get; set; }
    
    [JsonPropertyName("response")]
    public required string Response { get; set; }
}
