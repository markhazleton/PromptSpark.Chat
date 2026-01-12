using Microsoft.AspNetCore.Mvc;
using PromptSpark.Chat.WorkflowDomain;

namespace PromptSpark.Chat.Controllers.Api;

/// <summary>
/// API Controller for managing workflows and their nodes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly WorkflowService _workflowService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowController"/> class.
    /// </summary>
    /// <param name="workflowService">The workflow service for managing workflows.</param>
    public WorkflowController(WorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Loads a workflow from the specified file.
    /// </summary>
    /// <param name="workflowFileName">The name of the workflow file.</param>
    /// <returns>The loaded workflow object.</returns>
    [HttpPost("set")] // Changed route for clarity
    public IActionResult SetWorkflow([FromBody] string workflowFileName)
    {
        if (string.IsNullOrWhiteSpace(workflowFileName))
        {
            return BadRequest("Workflow file name cannot be null or empty.");
        }

        var workflow = _workflowService.LoadWorkflow(workflowFileName);
        if (workflow == null)
        {
            return NotFound($"Workflow file '{workflowFileName}' not found.");
        }

        return Ok(workflow);
    }

    /// <summary>
    /// Gets details of a specific workflow node by node ID.
    /// </summary>
    /// <param name="nodeId">The ID of the node to retrieve.</param>
    /// <returns>The node details including question and answers.</returns>
    [HttpGet("node/{nodeId}")]
    public IActionResult GetNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return BadRequest("Node ID cannot be null or empty.");
        }

        var workflow = _workflowService.LoadWorkflow("workflow.json"); // Example usage of workflow.json
        var node = workflow?.Nodes.FirstOrDefault(n => n.Id == nodeId);

        if (node == null)
        {
            return NotFound($"Node with ID '{nodeId}' not found.");
        }

        var response = new WorkflowNodeResponse
        {
            Question = node.Question,
            Answers = node.Answers.Select(answer => new AnswerOption
            {
                Response = answer.Response,
                Link = Url.Action(nameof(GetNode), new { nodeId = answer.NextNode }) ?? string.Empty
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Lists all available workflows.
    /// </summary>
    /// <returns>A list of workflow names.</returns>
    [HttpGet("workflows")]
    public IActionResult GetWorkflows()
    {
        var workflows = _workflowService.ListAvailableWorkflows();
        return Ok(workflows);
    }

    /// <summary>
    /// Adds a new node to the workflow.
    /// </summary>
    /// <param name="newNode">The details of the new node to add.</param>
    [HttpPost("node/add")]
    public IActionResult AddNode([FromBody] EditNodeViewModel newNode)
    {
        if (newNode == null)
        {
            return BadRequest("Node data cannot be null.");
        }

        _workflowService.AddNode(newNode);
        return Ok("Node added successfully.");
    }

    /// <summary>
    /// Updates an existing node in the workflow.
    /// </summary>
    /// <param name="updatedNode">The updated node details.</param>
    [HttpPut("node/update")]
    public IActionResult UpdateNode([FromBody] EditNodeViewModel updatedNode)
    {
        if (updatedNode == null)
        {
            return BadRequest("Node data cannot be null.");
        }

        _workflowService.UpdateNode(updatedNode);
        return Ok("Node updated successfully.");
    }

    /// <summary>
    /// Deletes a node from the workflow.
    /// </summary>
    /// <param name="nodeId">The ID of the node to delete.</param>
    [HttpDelete("node/delete/{nodeId}")]
    public IActionResult DeleteNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return BadRequest("Node ID cannot be null or empty.");
        }

        var workflow = _workflowService.LoadWorkflow("workflow.json");
        if (workflow == null)
        {
            return NotFound("Workflow not found.");
        }

        var node = workflow.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node == null)
        {
            return NotFound($"Node with ID '{nodeId}' not found.");
        }

        _workflowService.DeleteNode(new EditNodeViewModel { Id = nodeId, FileName = workflow.WorkFlowFileName ?? "workflow.json" });
        return Ok("Node deleted successfully.");
    }

    /// <summary>
    /// Saves the current workflow.
    /// </summary>
    [HttpPost("save")]
    public IActionResult SaveWorkflow()
    {
        var workflow = _workflowService.LoadWorkflow("workflow.json");
        _workflowService.SaveWorkflow(workflow);
        return Ok("Workflow saved successfully.");
    }

    /// <summary>
    /// Starts the workflow by redirecting to the start node.
    /// </summary>
    /// <returns>Redirection to the start node.</returns>
    [HttpGet("start")]
    public IActionResult StartWorkflow()
    {
        var workflow = _workflowService.LoadWorkflow("workflow.json");
        return RedirectToAction(nameof(GetNode), new { nodeId = workflow?.StartNode });
    }

    /// <summary>
    /// Returns a thank-you message for completing the workflow.
    /// </summary>
    /// <returns>A thank-you response.</returns>
    [HttpGet("end")]
    public IActionResult SayThanks()
    {
        var response = new WorkflowNodeResponse
        {
            Question = "Thanks for using our workflow service!",
            Answers = new List<AnswerOption>()
        };

        return Ok(response);
    }
}
