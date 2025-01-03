namespace PromptSpark.Chat.WorkflowDomain;

/// <summary>
/// Options class for configuring workflow paths.
/// </summary>
public class WorkflowOptions
{
    public string FilePath { get; set; } = "wwwroot/workflow.json";  // Default workflow file
    public string DirectoryPath { get; set; } = "wwwroot/workflows";  // Directory for workflow files
}
