namespace PromptSpark.Chat.ConversationDomain;

public class ChatEntry
{
    public required string BotResponse { get; set; }
    public DateTime Timestamp { get; set; }
    public required string User { get; set; }
    public required string UserMessage { get; set; }
}
