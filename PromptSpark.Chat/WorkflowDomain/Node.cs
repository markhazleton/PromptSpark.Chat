using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class Node
{
    public Node()
    {
        QuestionType = QuestionType.Options;
        Answers = new List<Answer>();
        Id = string.Empty;
        Question = string.Empty;
    }
    
    [JsonPropertyName("answers")]
    public List<Answer> Answers { get; set; }
    
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Id { get; set; }
    
    [JsonPropertyName("questionType")]
    public QuestionType QuestionType { get; set; }
    
    [JsonPropertyName("question")]
    public string Question { get; set; }
}
