using PromptSpark.Chat.WorkflowDomain;
namespace PromptSpark.Chat.ConversationDomain;

public class Conversation
{
    public Conversation()
    {
        // Initialize required properties with default values for parameterless constructor
        ConversationId = string.Empty;
        CurrentNodeId = string.Empty;
        UserName = "Anonymous";
        Workflow = null!; // Will be set by deserialization or explicit initialization
    }
    
    public Conversation(Workflow workflow, string conversationId, string? userName)
    {
        UserName = userName ?? "Anonymous";
        ConversationId = conversationId;
        Workflow = workflow;
        CurrentNodeId = workflow.StartNode;
    }

    public List<ChatEntry> ChatHistory { get; set; } = [];
    public string ConversationId { get; set; }
    public string CurrentNodeId { get; set; }
    public string PromptName { get; set; } = "helpful";
    public DateTime StartDate { get; set; } = DateTime.Now;
    public string UserName { get; set; }
    public Workflow Workflow { get; set; }
}
