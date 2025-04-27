using Microsoft.SemanticKernel;
using PromptSpark.Chat.Application.Diagnostics;
using PromptSpark.Chat.ConversationDomain;
using PromptSpark.Chat.WorkflowDomain;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure logging with more detailed options for debugging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Register services
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();

// Configure a named HttpClient called "workflow" with a base URL
builder.Services.AddHttpClient("workflow", client =>
{
    var urls = builder.Configuration.GetValue<string>("ASPNETCORE_URLS");
    var defaultHost = "localhost";
    var defaultPort = 7105;

    // Check if ASPNETCORE_URLS contains a full URL, and extract the host if so
    var uriBuilder = new UriBuilder
    {
        Scheme = "https",
        Host = defaultHost,
        Port = builder.Configuration.GetValue("ASPNETCORE_HTTPS_PORT", defaultPort),
        Path = "api/"
    };

    if (!string.IsNullOrEmpty(urls))
    {
        if (!string.IsNullOrEmpty(urls))
        {
            try
            {
                var firstUrl = urls.Split(';').FirstOrDefault();
                if (!string.IsNullOrEmpty(firstUrl))
                {
                    var uri = new Uri(firstUrl);
                    uriBuilder.Host = uri.Host;
                    uriBuilder.Port = uri.Port;
                }
            }
            catch (UriFormatException ex)
            {
                Log.Error(ex, "Error parsing ASPNETCORE_URLS");
            }
        }
    }
    client.BaseAddress = uriBuilder.Uri;
});


// ========================
// OpenAI Chat Completion Service
// ========================
string apikey = builder.Configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found";
string modelId = builder.Configuration.GetValue<string>("MODEL_ID") ?? "gpt-4o";

// Log OpenAI configuration before registration
Log.Information("Configuring OpenAI with Model ID: {ModelId}", modelId);
if (apikey == "not found")
{
    Log.Error("OpenAI API key not found in configuration");
}

// Add OpenAI chat completion with logging
try
{
    builder.Services.AddOpenAIChatCompletion(modelId, apikey);
    Log.Information("OpenAI chat completion service registered successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "Error registering OpenAI chat completion service");
}

builder.Services.Configure<WorkflowOptions>(builder.Configuration.GetSection("Workflow"));
builder.Services.AddSingleton<WorkflowService, WorkflowService>();

// Configure JsonSerializerOptions
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

// Register ConcurrentDictionaryService for Conversation
builder.Services.AddSingleton<ChatService, ChatService>();
builder.Services.AddSingleton<ConversationService>();
// Register PromptSparkHub with all dependencies injected
builder.Services.AddSingleton<PromptSparkHub>();


var app = builder.Build();

// Log service registrations at startup for debugging
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    // Use the new ConfigurationDiagnostics utility
    ConfigurationDiagnostics.LogOpenAIConfiguration(logger, app.Configuration);
    ConfigurationDiagnostics.LogServiceRegistrations(logger, serviceProvider);
}

// Middleware setup
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapOpenApi();
app.MapScalarApiReference();

// Top-level route registrations (replacing UseEndpoints)
app.MapControllers(); // Maps all API and MVC controllers automatically
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<PromptSparkHub>("/promptSparkHub");

app.Run();
