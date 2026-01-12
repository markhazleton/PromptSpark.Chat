using Microsoft.AspNetCore.SignalR;

namespace PromptSpark.Chat.ConversationDomain;

public class PromptSparkHub(
    ConversationService conversationService,
    ILogger<PromptSparkHub> logger) : Hub
{
    private const string STR_ChatBotName = "PromptSpark";

    public async Task SendMessage(string conversationId, string message)
    {
        CancellationToken ct = Context.ConnectionAborted;
        
        try
        {
            // Ensure conversation exists
            if (string.IsNullOrEmpty(conversationId))
            {
                logger.LogError("Invalid conversation ID: {ConversationId}", conversationId);
                await Clients.Caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, "Invalid conversation ID. Please refresh the page and try again.");
                return;
            }
            
            var conversation = conversationService.Lookup(conversationId);
            
            // Ensure workflow is loaded properly
            if (conversation.Workflow == null)
            {
                logger.LogError("No workflow found for conversation {ConversationId}", conversationId);
                await Clients.Caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, "No workflow available. Please select a workflow and try again.");
                return;
            }
            
            var sendArgument = await conversationService.ProcessUserResponse(conversationId, message, conversation, Clients.Caller, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during workflow progression for conversation {ConversationId}: {ErrorMessage}", conversationId, ex.Message);
            
            // Send more detailed error information to the client
            string errorMessage = "An error occurred while processing your request.";
            
            // Add more specific error messages based on exception type
            if (ex is InvalidOperationException)
            {
                errorMessage += " Invalid operation.";
            }
            else if (ex is NullReferenceException)
            {
                errorMessage += " Missing required data.";
            }
            else if (ex is TimeoutException)
            {
                errorMessage += " The operation timed out.";
            }
            
            await Clients.Caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, errorMessage);
            
            // Try to recover the conversation if possible
            try
            {
                var conversation = conversationService.Lookup(conversationId);
                conversationService.HandleWorkflowError(ex, conversation);
            }
            catch (Exception recoveryEx)
            {
                logger.LogError(recoveryEx, "Failed to recover from error in conversation {ConversationId}", conversationId);
            }
        }
    }

    public Task SetUserName(string conversationId, string userName, string workflowName)
    {
        try
        {
            // Input validation
            if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(workflowName))
            {
                logger.LogWarning("Invalid inputs for SetUserName. ConversationId: {ConversationId}, UserName: {UserName}, WorkflowName: {WorkflowName}",
                    conversationId, userName, workflowName);
                return Task.CompletedTask;
            }
            
            var conversation = conversationService.Lookup(conversationId);
            conversation.UserName = userName;
            
            // Load workflow with proper error handling
            try
            {
                conversation.Workflow = conversationService.LoadWorkflow(workflowName);
                if (conversation.CurrentNodeId != conversation.Workflow.StartNode)
                {
                    conversation.CurrentNodeId = conversation.Workflow.StartNode;
                }
                conversationService.Save(conversationId, conversation);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load workflow {WorkflowName} for conversation {ConversationId}", workflowName, conversationId);
                throw;
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in SetUserName for conversation {ConversationId}", conversationId);
            throw;
        }
    }
}
