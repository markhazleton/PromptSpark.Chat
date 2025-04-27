using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptSpark.Chat.ConversationDomain;

namespace PromptSpark.Chat.Application.Diagnostics;

/// <summary>
/// Utility class for logging configuration details to help with debugging.
/// </summary>
public static class ConfigurationDiagnostics
{
    /// <summary>
    /// Logs the current OpenAI configuration from appsettings.
    /// </summary>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void LogOpenAIConfiguration(ILogger logger, IConfiguration configuration)
    {
        try
        {
            var apiKey = configuration.GetValue<string>("OPENAI_API_KEY");
            var modelId = configuration.GetValue<string>("MODEL_ID");
            
            // Mask the API key for security
            string maskedApiKey = string.IsNullOrEmpty(apiKey) 
                ? "not configured" 
                : (apiKey.Length > 8 
                    ? apiKey.Substring(0, 4) + "..." + apiKey.Substring(apiKey.Length - 4) 
                    : "***");
            
            logger.LogInformation("OpenAI Configuration - Model ID: {ModelId}, API Key: {ApiKeyStatus}",
                string.IsNullOrEmpty(modelId) ? "not configured" : modelId,
                maskedApiKey);
            
            // Check for potential configuration issues
            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogError("OpenAI API key is not configured. Check OPENAI_API_KEY in appsettings.json or environment variables.");
            }
            
            if (string.IsNullOrEmpty(modelId))
            {
                logger.LogError("OpenAI Model ID is not configured. Check MODEL_ID in appsettings.json or environment variables.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while logging OpenAI configuration");
        }
    }
    
    /// <summary>
    /// Logs service registration information to help with debugging DI issues.
    /// </summary>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="serviceProvider">The application's service provider.</param>
    public static void LogServiceRegistrations(ILogger logger, IServiceProvider serviceProvider)
    {
        try
        {
            // Log key service registrations
            var chatServiceRegistered = serviceProvider.GetService<ChatService>() != null;
            var chatCompletionServiceRegistered = serviceProvider.GetService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>() != null;
            
            logger.LogInformation("Service registrations - ChatService: {ChatServiceRegistered}, " +
                                 "ChatCompletionService: {ChatCompletionServiceRegistered}",
                chatServiceRegistered, chatCompletionServiceRegistered);
            
            if (!chatCompletionServiceRegistered)
            {
                logger.LogError("ChatCompletionService is not registered. This will cause errors when trying to use OpenAI.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while logging service registrations");
        }
    }
}