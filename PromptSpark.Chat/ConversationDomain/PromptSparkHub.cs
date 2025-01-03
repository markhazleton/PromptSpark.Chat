using Microsoft.AspNetCore.SignalR;

namespace PromptSpark.Chat.ConversationDomain;

public class PromptSparkHub(
    IHubContext<PromptSparkHub> hubContext,
    ConversationService conversationService,
    ILogger<PromptSparkHub> logger) : Hub
{
    private const string STR_ChatBotName = "PromptSpark";

    public async Task SendMessage(string conversationId, string message)
    {
        CancellationToken ct = Context.ConnectionAborted;
        var conversation = conversationService.Lookup(conversationId);
        try
        {
            var sendArgument = await conversationService.ProcessUserResponse(conversationId, message, conversation, Clients.Caller, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during workflow progression for conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, "An error occurred while processing your request.");
            conversationService.HandleWorkflowError(ex, conversation);
        }
    }

    public Task SetUserName(string conversationId, string userName, string workflowName)
    {
        var conversation = conversationService.Lookup(conversationId);
        conversation.UserName = userName;
        conversation.Workflow = conversationService.LoadWorkflow(workflowName);
        if (conversation.CurrentNodeId != conversation.Workflow.StartNode)
        {
            conversation.CurrentNodeId = conversation.Workflow.StartNode;
        }
        conversationService.Save(conversationId, conversation);
        return Task.CompletedTask;
    }
}
