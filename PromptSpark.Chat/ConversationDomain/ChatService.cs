using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
namespace PromptSpark.Chat.ConversationDomain;


public class ChatService 
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IChatCompletionService chatCompletionService, ILogger<ChatService> logger)
    {
        _chatCompletionService = chatCompletionService;
        _logger = logger;
    }

    public async Task<string> GenerateBotResponse(ChatHistory chatHistory)
    {
        var response = new StringBuilder();
        
        if (chatHistory == null)
        {
            _logger.LogError("Chat history is null in GenerateBotResponse");
            return "Error: No conversation context available.";
        }
        
        _logger.LogInformation("Generating bot response for chat history with {MessageCount} messages", chatHistory.Count);
        
        // Log the last user message to help with debugging
        var lastUserMessage = chatHistory.LastOrDefault(m => m.Role == AuthorRole.User)?.Content;
        _logger.LogInformation("Responding to user message: {UserMessage}", lastUserMessage ?? "No user message found");

        try
        {
            int chunkCount = 0;
            await foreach (var content in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                chunkCount++;
                if (content?.Content != null)
                {
                    response.Append(content.Content);
                    
                    // Log progress but avoid excessive logging
                    if (chunkCount % 10 == 0)
                    {
                        _logger.LogDebug("Received {ChunkCount} chunks in GenerateBotResponse", chunkCount);
                    }
                }
                else
                {
                    _logger.LogWarning("Received null content in chunk #{ChunkNumber} in GenerateBotResponse", chunkCount);
                }
            }
            
            _logger.LogInformation("Successfully generated bot response with {ChunkCount} chunks, total length: {ResponseLength}", 
                chunkCount, response.Length);
                
            if (response.Length == 0)
            {
                _logger.LogWarning("Generated an empty response");
                return "I couldn't generate a response. Please try asking your question differently.";
            }
                
            return response.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bot response.");
            
            // Log more detailed information about the exception
            _logger.LogError("Exception type: {ExceptionType}", ex.GetType().FullName);
            
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerExceptionType}: {InnerExceptionMessage}", 
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
            }
            
            // Include stack trace in logs for debugging
            _logger.LogDebug("Stack trace: {StackTrace}", ex.StackTrace);
            
            // Return a more specific error message based on the exception type
            if (ex is HttpRequestException)
            {
                return "Unable to connect to the AI service. Please try again later.";
            }
            else if (ex is TimeoutException)
            {
                return "The request timed out. Please try a shorter message or try again later.";
            }
            else if (ex is ArgumentException)
            {
                return "There was a problem with your request. Please try rephrasing your message.";
            }
            
            return "An error occurred while generating a response. Please try again.";
        }
    }

    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)
    {
        if (chatHistory == null) throw new ArgumentNullException(nameof(chatHistory));
        if (clients == null) throw new ArgumentNullException(nameof(clients));
        if (string.IsNullOrEmpty(conversationId)) throw new ArgumentException("Conversation ID cannot be null or empty.", nameof(conversationId));

        try
        {
            _logger.LogInformation("Starting chat engagement for conversation {ConversationId}", conversationId);
            _logger.LogDebug("Chat history has {MessageCount} messages", chatHistory.Count);
            
            // Log the last user message to help with debugging
            var lastUserMessage = chatHistory.LastOrDefault(m => m.Role == AuthorRole.User)?.Content;
            _logger.LogInformation("Processing user message: {UserMessage}", lastUserMessage ?? "No user message found");
            
            // Create a unique message ID for this chat exchange
            string messageId = Guid.NewGuid().ToString();
            _logger.LogDebug("Generated messageId: {MessageId} for conversation {ConversationId}", messageId, conversationId);
            
            int chunkCount = 0;
            try 
            {
                await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory).WithCancellation(cancellationToken))
                {
                    chunkCount++;
                    if (response?.Content != null)
                    {
                        await clients.SendAsync("ReceiveMessage", "PromptSpark", response.Content, messageId, cancellationToken);
                        
                        // Log every 10th chunk to avoid excessive logging
                        if (chunkCount % 10 == 0)
                        {
                            _logger.LogDebug("Sent chunk #{ChunkNumber} for message {MessageId}", chunkCount, messageId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Received null content in chunk #{ChunkNumber} for message {MessageId}", chunkCount, messageId);
                    }
                }
                
                _logger.LogInformation("Successfully streamed {ChunkCount} chunks for conversation {ConversationId}", chunkCount, conversationId);
                
                // Send empty string to signal end of streaming response
                await clients.SendAsync("ReceiveMessage", "PromptSpark", "", messageId, cancellationToken);
                _logger.LogDebug("Sent end-of-stream signal for message {MessageId}", messageId);
            }
            catch (Exception streamEx)
            {
                _logger.LogError(streamEx, "Error during streaming for conversation {ConversationId} after {ChunkCount} chunks: {ErrorMessage}", 
                    conversationId, chunkCount, streamEx.Message);
                
                // Include inner exception details if present
                if (streamEx.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerErrorType}: {InnerErrorMessage}", 
                        streamEx.InnerException.GetType().Name, streamEx.InnerException.Message);
                }
                
                throw; // Rethrow to be caught by the outer try-catch
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("EngageChatAgent operation was canceled for conversation {ConversationId}", conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error engaging chat agent for conversation {ConversationId}: {ErrorMessage}", 
                conversationId, ex.Message);
            
            // Log more detailed information about the exception
            _logger.LogError("Exception type: {ExceptionType}", ex.GetType().FullName);
            
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerExceptionType}: {InnerExceptionMessage}", 
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
            }
            
            // Include stack trace in logs for debugging
            _logger.LogDebug("Stack trace: {StackTrace}", ex.StackTrace);
            
            // Create a unique message ID for the error message
            string errorMessageId = Guid.NewGuid().ToString();
            _logger.LogDebug("Generated error messageId: {ErrorMessageId}", errorMessageId);
            
            // Send error message with proper messageId parameter
            string errorMessage = "An error occurred while processing your request. Please try again or use a different query.";
            
            // Add more context to the error message for better user experience
            if (ex is HttpRequestException)
            {
                errorMessage = "Unable to connect to the AI service. Please try again later.";
            }
            else if (ex is TimeoutException)
            {
                errorMessage = "The request timed out. Please try a shorter message or try again later.";
            }
            else if (ex is InvalidOperationException)
            {
                errorMessage = "An error occurred in the workflow. Please try restarting the conversation.";
            }
            
            _logger.LogDebug("Sending error message to client: {ErrorMessage}", errorMessage);
            await clients.SendAsync("ReceiveMessage", "PromptSpark", errorMessage, errorMessageId, cancellationToken);
        }
    }
}
