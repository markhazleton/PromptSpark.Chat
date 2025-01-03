using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class AnswerOption
{

    [JsonPropertyName("link")]
    public string Link { get; set; }
    [JsonPropertyName("response")]
    public string Response { get; set; }
}
