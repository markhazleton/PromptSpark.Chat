using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class WorkflowNodeResponse
{

    [JsonPropertyName("answers")]
    public List<AnswerOption> Answers { get; set; } = new();
    
    [JsonPropertyName("question")]
    public required string Question { get; set; }
}
