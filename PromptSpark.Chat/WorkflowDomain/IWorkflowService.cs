namespace PromptSpark.Chat.WorkflowDomain;

/// <summary>
/// Interface defining methods for managing workflows and their nodes.
/// </summary>
public interface IWorkflowService
{
    Workflow LoadWorkflow(string fileName);
    EditNodeViewModel LoadNode(string nodeId, string fileName);
    void AddNode(EditNodeViewModel newNode);
    void UpdateNode(EditNodeViewModel updatedNode);
    void DeleteNode(EditNodeViewModel node);
    void SaveWorkflow(Workflow workflow);
    List<string> ListAvailableWorkflows();
}