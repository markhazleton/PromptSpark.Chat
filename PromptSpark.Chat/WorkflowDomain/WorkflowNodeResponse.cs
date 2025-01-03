using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class WorkflowNodeResponse
{

    [JsonPropertyName("answers")]
    public List<AnswerOption> Answers { get; set; }
    [JsonPropertyName("question")]
    public string Question { get; set; }
}
