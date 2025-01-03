using Microsoft.Extensions.Options;
using System.Text.Json;

namespace PromptSpark.Chat.WorkflowDomain;

/// <summary>
/// Service class for managing workflows and their nodes.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WorkflowOptions _options;

    public WorkflowService(IOptions<WorkflowOptions> options, JsonSerializerOptions jsonOptions)
    {
        _options = options.Value;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Loads a node from a specified workflow file by its ID.
    /// </summary>
    public EditNodeViewModel LoadNode(string nodeId, string fileName)
    {
        var workflow = LoadWorkflow(fileName);
        var node = workflow.Nodes.Find(n => n.Id == nodeId);
        return node == null
            ? throw new InvalidOperationException($"Node with ID {nodeId} does not exist in workflow {fileName}.")
            : new EditNodeViewModel()
            {
                Id = node.Id,
                Question = node.Question,
                Answers = node.Answers,
                FileName = fileName,
                QuestionType = node.QuestionType
            };
    }

    /// <summary>
    /// Loads a workflow from a specified file.
    /// </summary>
    public Workflow LoadWorkflow(string fileName)
    {
        var filePath = fileName ?? "workflow.json";
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), _options.DirectoryPath, filePath);

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Workflow file '{filePath}' not found in '{_options.DirectoryPath}'");

        var jsonTemplate = File.ReadAllText(jsonPath);
        var workflow = JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions)
                      ?? throw new InvalidOperationException("Failed to load Workflow configuration.");

        workflow.WorkFlowName = Path.GetFileNameWithoutExtension(filePath);
        workflow.WorkFlowFileName = filePath;
        return workflow;
    }

    /// <summary>
    /// Lists all available workflow files in the configured directory.
    /// </summary>
    public List<string> ListAvailableWorkflows()
    {
        var list = new List<string>();
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _options.DirectoryPath);

        if (!Directory.Exists(directoryPath))
            return list;

        foreach (var file in Directory.GetFiles(directoryPath, "*.json"))
        {
            list.Add(Path.GetFileName(file));
        }

        return list;
    }

    /// <summary>
    /// Adds a new node to a workflow or updates an existing node if it already exists.
    /// </summary>
    public void AddNode(EditNodeViewModel newNode)
    {
        var workflow = LoadWorkflow(newNode.FileName) ?? throw new InvalidOperationException($"Workflow not found {newNode.FileName}");
        var node = workflow.Nodes.Find(n => n.Id == newNode.Id);
        if (node == null)
        {
            workflow.Nodes.Add(newNode);
        }
        else
        {
            node.Answers = newNode.Answers;
            node.Question = newNode.Question;
            node.QuestionType = newNode.QuestionType;
        }
        SaveWorkflow(workflow);
    }

    /// <summary>
    /// Updates an existing node in a workflow.
    /// </summary>
    public void UpdateNode(EditNodeViewModel updatedNode)
    {
        var workflow = LoadWorkflow(updatedNode.FileName);
        var existingNodeIndex = workflow.Nodes.FindIndex(n => n.Id == updatedNode.Id);
        if (existingNodeIndex == -1)
            throw new InvalidOperationException($"Node with ID {updatedNode.Id} does not exist.");

        workflow.Nodes[existingNodeIndex] = updatedNode.GetNode();
        SaveWorkflow(workflow);
    }

    /// <summary>
    /// Deletes a node from a workflow and updates references to the deleted node.
    /// </summary>
    public void DeleteNode(EditNodeViewModel node)
    {
        var workflow = LoadWorkflow(node.FileName);
        var nodeToRemove = workflow.Nodes.Find(n => n.Id == node.Id);
        if (nodeToRemove == null)
            throw new InvalidOperationException($"Node with ID {node.Id} does not exist in workflow {node.FileName}.");

        workflow.Nodes.Remove(nodeToRemove);
        UpdateReferencesAfterNodeDeletion(workflow, nodeToRemove.Id);
        SaveWorkflow(workflow);
    }

    /// <summary>
    /// Saves the workflow to its respective file.
    /// </summary>
    public void SaveWorkflow(Workflow workflow)
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _options.DirectoryPath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = string.IsNullOrEmpty(workflow.WorkFlowFileName) ? "workflow.json" : workflow.WorkFlowFileName;
        var jsonPath = Path.Combine(directoryPath, fileName);
        var json = JsonSerializer.Serialize(workflow, _jsonOptions);

        File.WriteAllText(jsonPath, json);
    }

    /// <summary>
    /// Updates references in the workflow after a node is deleted.
    /// </summary>
    private void UpdateReferencesAfterNodeDeletion(Workflow workflow, string deletedNodeId)
    {
        foreach (var node in workflow.Nodes)
        {
            node.Answers.RemoveAll(answer => answer.NextNode == deletedNodeId);
        }
    }
}
