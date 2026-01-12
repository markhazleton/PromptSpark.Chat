using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Chat.WorkflowDomain;

public class Workflow
{

    [JsonPropertyName("nodes")]
    public List<Node> Nodes { get; set; } = new();
    
    [JsonPropertyName("startNode")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public required string StartNode { get; set; }
    
    [JsonPropertyName("workflowId")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public required string WorkflowId { get; set; }
    
    [JsonPropertyName("workflowName")]
    public string WorkFlowName { get; set; } = "workflow";
    
    public string? WorkFlowFileName { get; internal set; }

    internal string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
