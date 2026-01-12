using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Chat.WorkflowDomain;

namespace PromptSpark.Chat.ConversationDomain;

/// <summary>
/// Service for managing conversations, including chat history, workflow progression, and bot responses.
/// </summary>
public class ConversationService(WorkflowService workflowService, ChatService chatService, ILogger<ConversationService> logger) : ConcurrentDictionaryService<Conversation>
{
    private const string STR_ChatBotName = "PromptSpark";
    private readonly ChatService _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
    private readonly WorkflowService _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
    private readonly AdaptiveCardService _adaptiveCardService = new AdaptiveCardService();

    /// <summary>
    /// Adds a chat entry to the conversation's chat history.
    /// </summary>
    /// <param name="conversation">The conversation to add the entry to.</param>
    /// <param name="user">The user who sent the message.</param>
    /// <param name="message">The message content.</param>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="botResponse">The bot's response to the message.</param>
    public void AddChatEntry(Conversation conversation, string user, string message, DateTime timestamp, string botResponse = "")
    {
        conversation.ChatHistory.Add(new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = botResponse
        });
    }

    /// <summary>
    /// Builds a chat history object from a conversation.
    /// </summary>
    /// <param name="conversation">The conversation to build the chat history from.</param>
    /// <returns>A ChatHistory object containing the conversation's chat history.</returns>
    public ChatHistory BuildChatHistoryFromConversation(Conversation conversation)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions, ask if ready for full answer.");
        foreach (var chatEntry in conversation.ChatHistory)
        {
            if (!string.IsNullOrEmpty(chatEntry.UserMessage))
                chatHistory.AddUserMessage(chatEntry.UserMessage);
            if (!string.IsNullOrEmpty(chatEntry.BotResponse))
                chatHistory.AddSystemMessage(chatEntry.BotResponse);
        }
        return chatHistory;
    }

    /// <summary>
    /// Engages the chat agent with the provided chat history.
    /// </summary>
    /// <param name="chatHistory">The chat history to engage the agent with.</param>
    /// <param name="conversationId">The ID of the conversation.</param>
    /// <param name="clients">The clients to send the response to.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)
    {
        await _chatService.EngageChatAgent(chatHistory, conversationId, clients, cancellationToken);
    }

    /// <summary>
    /// Generates a bot response based on the provided chat history.
    /// </summary>
    /// <param name="chatHistory">The chat history to generate a response from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the bot response.</returns>
    public async Task<string> GenerateBotResponse(ChatHistory chatHistory, CancellationToken cancellationToken)
    {
        return await _chatService.GenerateBotResponse(chatHistory);
    }

    /// <summary>
    /// Gets the current node in the conversation's workflow.
    /// </summary>
    /// <param name="conversation">The conversation to get the current node from.</param>
    /// <returns>The current node, or null if not found.</returns>
    public Node? GetCurrentNode(Conversation conversation)
    {
        return conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
    }

    /// <summary>
    /// Handles errors that occur during workflow progression.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="conversation">The conversation in which the error occurred.</param>
    public void HandleWorkflowError(Exception ex, Conversation conversation)
    {
        logger.LogError(ex, "Error in workflow progression for conversation {ConversationId}.", conversation.ConversationId);
        
        try
        {
            // Attempt to recover the conversation by ensuring we're at a valid node
            if (conversation.Workflow != null && conversation.Workflow.Nodes.Any())
            {
                // Check if the current node exists
                var currentNodeExists = conversation.Workflow.Nodes.Any(node => node.Id == conversation.CurrentNodeId);
                
                if (!currentNodeExists)
                {
                    // Reset to start node if current node is invalid
                    logger.LogInformation("Resetting conversation {ConversationId} to start node due to invalid current node", 
                        conversation.ConversationId);
                    conversation.CurrentNodeId = conversation.Workflow.StartNode;
                    
                    // Save the recovered state
                    Save(conversation.ConversationId, conversation);
                }
            }
            else
            {
                logger.LogWarning("Cannot recover conversation {ConversationId} - workflow is null or has no nodes", 
                    conversation.ConversationId);
            }
        }
        catch (Exception recoveryEx)
        {
            logger.LogError(recoveryEx, "Failed to recover from error in conversation {ConversationId}", 
                conversation.ConversationId);
        }
    }

    /// <summary>
    /// Looks up a conversation by its ID, or creates a new one if it does not exist.
    /// </summary>
    /// <param name="conversationId">The ID of the conversation to look up.</param>
    /// <returns>The conversation with the specified ID.</returns>
    public override Conversation Lookup(string conversationId)
    {
        return GetOrAdd(conversationId, id =>
        {
            var workflow = _workflowService.LoadWorkflow("workflow.json");
            return new Conversation(workflow, id, null);
        });
    }

    /// <summary>
    /// Loads a specific workflow by name.
    /// </summary>
    /// <param name="workflowName">The name of the workflow to load.</param>
    /// <returns>The loaded workflow.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the workflow could not be loaded.</exception>
    public Workflow LoadWorkflow(string workflowName)
    {
        try
        {
            return _workflowService.LoadWorkflow(workflowName)
                   ?? throw new InvalidOperationException($"Workflow '{workflowName}' could not be loaded.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load workflow '{WorkflowName}'", workflowName);
            throw;
        }
    }

    /// <summary>
    /// Processes a message sent by the user.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="conversationId">The ID of the conversation.</param>
    /// <param name="conversation">The conversation to process the message for.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message type and data.</returns>
    public async Task<(MessageType messageType, object messageData)?> ProcessSendMessage(
        string message,
        string conversationId,
        Conversation conversation,
        CancellationToken ct)
    {
        var user = conversation.UserName ?? STR_ChatBotName;
        var timestamp = DateTime.Now;

        AddChatEntry(conversation, user, message, timestamp);

        var chatHistory = BuildChatHistoryFromConversation(conversation);
        chatHistory.AddUserMessage(message);
        var botResponse = await GenerateBotResponse(chatHistory, ct);

        AddChatEntry(conversation, STR_ChatBotName, message, timestamp, botResponse);

        return (MessageType.ReceiveMessage, new { User = user, Message = message, ConversationId = conversationId });
    }

    /// <summary>
    /// Processes a response from the user.
    /// </summary>
    /// <param name="conversationId">The ID of the conversation.</param>
    /// <param name="userResponse">The user's response.</param>
    /// <param name="conversation">The conversation to process the response for.</param>
    /// <param name="caller">The client proxy to send the response to.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message type and data.</returns>
    public async Task<(MessageType messageType, object messageData)?> ProcessUserResponse(
        string conversationId,
        string userResponse,
        Conversation conversation,
        ISingleClientProxy caller,
        CancellationToken ct)
    {
        // Validate conversation
        if (conversation == null)
        {
            logger.LogError("Conversation is null for conversation ID {ConversationId}", conversationId);
            await caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, "Session error. Please refresh the page and try again.", 
                cancellationToken: ct);
            return null;
        }

        try
        {
            logger.LogInformation("Processing user response for conversation {ConversationId}: {UserResponse}", 
                conversationId, userResponse?.Length > 50 ? userResponse.Substring(0, 50) + "..." : userResponse);
                
            // Log conversation state
            logger.LogDebug("Conversation state - CurrentNodeId: {CurrentNodeId}, UserName: {UserName}, ChatHistory: {ChatHistoryCount} entries", 
                conversation.CurrentNodeId, conversation.UserName, conversation.ChatHistory.Count);
                
            var currentNode = GetCurrentNode(conversation);
            if (currentNode == null)
            {
                logger.LogError("Current node not found for conversation {ConversationId} with node ID {NodeId}", 
                    conversationId, conversation.CurrentNodeId);
                    
                // Log available nodes to help diagnose the issue
                if (conversation.Workflow?.Nodes != null)
                {
                    logger.LogDebug("Available nodes: {NodeIds}", string.Join(", ", conversation.Workflow.Nodes.Select(n => n.Id)));
                }
                else
                {
                    logger.LogError("Workflow or nodes collection is null for conversation {ConversationId}", conversationId);
                }
                
                await caller.SendAsync(MessageType.ReceiveMessage.ToString(), 
                    STR_ChatBotName, "Error in workflow progression. The current step could not be found.", 
                    cancellationToken: ct);
                
                return (MessageType.ReceiveMessage, new { sender = STR_ChatBotName, content = "Error in workflow progression." });
            }

            logger.LogDebug("Current node: {NodeId}, Question: {Question}, AnswerCount: {AnswerCount}", 
                currentNode.Id, currentNode.Question, currentNode.Answers?.Count ?? 0);
                
            if (currentNode.Answers != null)
            {
                logger.LogDebug("Available answers for node {NodeId}: {Answers}", 
                    currentNode.Id, string.Join(", ", currentNode.Answers.Select(a => a.Response)));
            }

            var adaptiveCardJson = _adaptiveCardService.GetAdaptiveCardForNode(currentNode);
            if (string.IsNullOrEmpty(adaptiveCardJson))
            {
                logger.LogWarning("Generated adaptive card is empty for conversation {ConversationId}, node {NodeId}", 
                    conversationId, currentNode.Id);
            }
            else
            {
                logger.LogDebug("Generated adaptive card JSON for node {NodeId} with length {JsonLength}", 
                    currentNode.Id, adaptiveCardJson.Length);
            }

            if (!string.IsNullOrWhiteSpace(userResponse))
            {
                // Record the interaction
                AddChatEntry(conversation, conversation.UserName ?? STR_ChatBotName, userResponse, DateTime.Now, currentNode.Question);
                var matchingAnswer = currentNode?.Answers?.FirstOrDefault(answer => 
                    answer.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));

                // Log if we found a matching answer
                if (matchingAnswer != null)
                {
                    logger.LogInformation("Found matching answer for user response in conversation {ConversationId}: {Response} → NextNode: {NextNode}", 
                        conversationId, matchingAnswer.Response, matchingAnswer.NextNode);
                }
                else
                {
                    logger.LogInformation("No matching answer found for user response in conversation {ConversationId}: {UserResponse}", 
                        conversationId, userResponse);
                }

                if (matchingAnswer is null)
                {
                    try
                    {
                        // Free-form text response handling
                        logger.LogInformation("Processing free-form text response for conversation {ConversationId}", conversationId);
                        
                        // Send acknowledgment message to user
                        await caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, 
                            "I'm processing your message...", cancellationToken: ct);
                        
                        var chatHistory = BuildChatHistoryFromConversation(conversation);
                        chatHistory.AddUserMessage(userResponse);
                        
                        logger.LogDebug("Built chat history with {MessageCount} messages for conversation {ConversationId}", 
                            chatHistory.Count, conversationId);

                        // Log OpenAI configuration for debugging
                        try
                        {
                            var config = typeof(Program).Assembly.GetTypes()
                                .FirstOrDefault(t => t.Name == "Program")
                                ?.GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                                ?.GetParameters()
                                .FirstOrDefault()
                                ?.ParameterType.Assembly.GetType("Microsoft.Extensions.Configuration.ConfigurationExtensions");
                                
                            logger.LogDebug("OpenAI configuration check attempted");
                        }
                        catch (Exception configEx)
                        {
                            logger.LogWarning(configEx, "Error checking OpenAI configuration");
                        }

                        try
                        {
                            // Engage the chat agent with the user's message
                            logger.LogInformation("Engaging chat agent for conversation {ConversationId}", conversationId);
                            await EngageChatAgent(chatHistory, conversationId, caller, ct);
                            logger.LogInformation("Chat agent engagement completed for conversation {ConversationId}", conversationId);
                        }
                        catch (Exception chatEx)
                        {
                            logger.LogError(chatEx, "Error during chat agent engagement for conversation {ConversationId}: {ErrorMessage}", 
                                conversationId, chatEx.Message);
                            throw; // Let the outer catch block handle this
                        }
                        
                        // Wait briefly to ensure messages are displayed in correct order
                        await Task.Delay(200, ct);
                        
                        // After the chat agent response, send the adaptive card again to continue the workflow
                        logger.LogDebug("Sending adaptive card after chat agent response for conversation {ConversationId}", conversationId);
                        await caller.SendAsync(MessageType.ReceiveAdaptiveCard.ToString(), adaptiveCardJson, cancellationToken: ct);
                        
                        // Save the updated conversation state
                        Save(conversationId, conversation);
                        logger.LogDebug("Saved conversation state for {ConversationId}", conversationId);
                        
                        return (MessageType.EngageChatAgent, new { chatHistory, conversationId });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error handling free-form text for conversation {ConversationId}: {ErrorMessage}", 
                            conversationId, ex.Message);
                            
                        if (ex.InnerException != null)
                        {
                            logger.LogError("Inner exception: {InnerExceptionType}: {InnerExceptionMessage}", 
                                ex.InnerException.GetType().FullName, ex.InnerException.Message);
                        }
                            
                        await caller.SendAsync(MessageType.ReceiveMessage.ToString(), 
                            STR_ChatBotName, "I couldn't process your message properly. Please try again or use one of the provided options.", 
                            cancellationToken: ct);
                            
                        // Still send the adaptive card to allow the user to continue
                        await caller.SendAsync(MessageType.ReceiveAdaptiveCard.ToString(), adaptiveCardJson, cancellationToken: ct);
                        return null;
                    }
                }

                // Process structured workflow response
                logger.LogInformation("Processing structured workflow response for conversation {ConversationId}", conversationId);
                var nextNode = ProgressWorkflow(conversation, userResponse);
                
                if (nextNode == null)
                {
                    logger.LogError("Next node not found after progressing workflow for conversation {ConversationId}", conversationId);
                    
                    // Log more details about the current state
                    logger.LogDebug("Current node ID after progression attempt: {CurrentNodeId}", conversation.CurrentNodeId);
                    if (conversation.Workflow?.Nodes != null)
                    {
                        logger.LogDebug("Available nodes: {NodeIds}", string.Join(", ", conversation.Workflow.Nodes.Select(n => n.Id)));
                    }
                    
                    await caller.SendAsync(MessageType.ReceiveMessage.ToString(), 
                        STR_ChatBotName, "Error finding the next step in the workflow. Please try again.", 
                        cancellationToken: ct);
                    
                    return (MessageType.ReceiveMessage, new { sender = STR_ChatBotName, content = "Error in workflow progression." });
                }

                logger.LogInformation("Progressed to next node {NodeId} for conversation {ConversationId}", nextNode.Id, conversationId);
                adaptiveCardJson = _adaptiveCardService.GetAdaptiveCardForNode(nextNode);
                logger.LogDebug("Generated adaptive card for next node {NodeId} with length {JsonLength}", 
                    nextNode.Id, adaptiveCardJson?.Length ?? 0);
            }
            
            // Save conversation state
            Save(conversationId, conversation);
            logger.LogDebug("Saved conversation state for {ConversationId}", conversationId);
            
            // Send the adaptive card
            await caller.SendAsync(MessageType.ReceiveAdaptiveCard.ToString(), adaptiveCardJson, cancellationToken: ct);

            logger.LogInformation("AdaptiveCard sent for conversation {ConversationId}, node {NodeId}", 
                conversationId, conversation.CurrentNodeId);
            
            return (MessageType.ReceiveAdaptiveCard, adaptiveCardJson ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing user response for conversation {ConversationId}: {ErrorMessage}", 
                conversationId, ex.Message);

            // Log more detailed information about the exception
            logger.LogError("Exception type: {ExceptionType}", ex.GetType().FullName);
            
            if (ex.InnerException != null)
            {
                logger.LogError("Inner exception: {InnerExceptionType}: {InnerExceptionMessage}", 
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
            }
            
            // Include stack trace in logs for debugging
            logger.LogDebug("Stack trace: {StackTrace}", ex.StackTrace);
                
            await caller.SendAsync(MessageType.ReceiveMessage.ToString(), 
                STR_ChatBotName, "An unexpected error occurred. Please try again or restart the conversation.", 
                cancellationToken: ct);
                
            return null;
        }
    }

    /// <summary>
    /// Progresses the workflow based on the user's response.
    /// </summary>
    /// <param name="conversation">The conversation to progress the workflow for.</param>
    /// <param name="userResponse">The user's response.</param>
    /// <returns>The next node in the workflow, or null if an error occurred.</returns>
    public Node? ProgressWorkflow(Conversation conversation, string userResponse)
    {
        var currentNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
        if (currentNode == null)
        {
            logger.LogError("Node with ID '{CurrentNodeId}' not found.", conversation.CurrentNodeId);
            return null;
        }
        
        // Check if the response matches any available answers for the current node
        var selectedAnswer = currentNode.Answers.FirstOrDefault(a => a.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));
        
        if (selectedAnswer != null)
        {
            // Valid workflow response found, proceed to next node
            if (!string.IsNullOrEmpty(selectedAnswer.NextNode))
            {
                conversation.CurrentNodeId = selectedAnswer.NextNode;
                logger.LogInformation("Workflow progressed from node {CurrentNodeId} to node {NextNodeId}", 
                    currentNode.Id, selectedAnswer.NextNode);
            }
            else
            {
                logger.LogWarning("Selected answer has no next node specified for node {CurrentNodeId}", currentNode.Id);
            }
        }
        else
        {
            // No matching answer found, but we'll keep the user at the current node
            logger.LogInformation("No matching answer found for response '{Response}' at node {CurrentNodeId}", 
                userResponse, currentNode.Id);
        }
        
        // Try to find and return the next node
        var nextNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
        if (nextNode == null)
        {
            logger.LogError("Next node with ID '{NextNodeId}' not found.", conversation.CurrentNodeId);
        }
        
        return nextNode;
    }
}
