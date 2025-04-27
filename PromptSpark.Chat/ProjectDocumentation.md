# Project Documentation

## Class: [AdaptiveCardService](ConversationDomain/AdaptiveCardService.cs)
Namespace: `Global`

### Public Methods:
- `GetAdaptiveCardForNode(Node currentNode)`: Returns `string` ****
- `GetCardWithAnswers(Node currentNode)`: Returns `string` ****
- `GetCardWithText(Node currentNode)`: Returns `string` ****
- `GetMessageWriteCard()`: Returns `string` ****
- `GetCardWithAnswersAndText(Node currentNode)`: Returns `string` ****

---
## Class: [ConversationService](ConversationDomain/ConversationService.cs)
Namespace: `Global`

**Service for managing conversations, including chat history, workflow progression, and bot responses.**

- Inherits: `ConcurrentDictionaryService<Conversation>`

### Public Methods:
- `AddChatEntry(Conversation conversation, string user, string message, DateTime timestamp, string botResponse)`: Returns `void` **Adds a chat entry to the conversation's chat history.**
- `BuildChatHistoryFromConversation(Conversation conversation)`: Returns `ChatHistory` **Builds a chat history object from a conversation.**
- `EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)`: Returns `Task` **Engages the chat agent with the provided chat history.**
- `GenerateBotResponse(ChatHistory chatHistory, CancellationToken cancellationToken)`: Returns `Task<string>` **Generates a bot response based on the provided chat history.**
- `GetCurrentNode(Conversation conversation)`: Returns `Node?` **Gets the current node in the conversation's workflow.**
- `HandleWorkflowError(Exception ex, Conversation conversation)`: Returns `void` **Handles errors that occur during workflow progression.**
- `Lookup(string conversationId)`: Returns `Conversation` **Looks up a conversation by its ID, or creates a new one if it does not exist.**
- `LoadWorkflow(string workflowName)`: Returns `Workflow` **Loads a specific workflow by name.**
- `ProcessSendMessage(string message, string conversationId, Conversation conversation, CancellationToken ct)`: Returns `Task<(MessageType messageType, object messageData)?>` **Processes a message sent by the user.**
- `ProcessUserResponse(string conversationId, string userResponse, Conversation conversation, ISingleClientProxy caller, CancellationToken ct)`: Returns `Task<(MessageType messageType, object messageData)?>` **Processes a response from the user.**
- `ProgressWorkflow(Conversation conversation, string userResponse)`: Returns `Node?` **Progresses the workflow based on the user's response.**

---
## Class: [WorkflowService](WorkflowDomain/WorkflowService.cs)
Namespace: `Global`

**Service class for managing workflows and their nodes.**

- Inherits: `IWorkflowService`

- Implements: `IWorkflowService`

### Public Methods:
- `LoadNode(string nodeId, string fileName)`: Returns `EditNodeViewModel` **Loads a node from a specified workflow file by its ID.**
- `LoadWorkflow(string fileName)`: Returns `Workflow` **Loads a workflow from a specified file.**
- `ListAvailableWorkflows()`: Returns `List<string>` **Lists all available workflow files in the configured directory.**
- `AddNode(EditNodeViewModel newNode)`: Returns `void` **Adds a new node to a workflow or updates an existing node if it already exists.**
- `UpdateNode(EditNodeViewModel updatedNode)`: Returns `void` **Updates an existing node in a workflow.**
- `DeleteNode(EditNodeViewModel node)`: Returns `void` **Deletes a node from a workflow and updates references to the deleted node.**
- `SaveWorkflow(Workflow workflow)`: Returns `void` **Saves the workflow to its respective file.**

### Private Methods:
- `UpdateReferencesAfterNodeDeletion(Workflow workflow, string deletedNodeId)`: Returns `void` **Updates references in the workflow after a node is deleted.**

---
## Class: [WorkflowController](Controllers/Api/WorkflowController.cs)
Namespace: `Global`

**API Controller for managing workflows and their nodes.**

- Inherits: `ControllerBase`

- Implements: `ControllerBase`

- Route Prefix: `api/workflow`

### Public Methods:
- `SetWorkflow(string workflowFileName)`: Returns `IActionResult` **Loads a workflow from the specified file.**
  - Route: `POST api/workflow/set`
- `GetNode(string nodeId)`: Returns `IActionResult` **Gets details of a specific workflow node by node ID.**
  - Route: `GET api/workflow/node/{nodeId}`
- `GetWorkflows()`: Returns `IActionResult` **Lists all available workflows.**
  - Route: `GET api/workflow/workflows`
- `AddNode(EditNodeViewModel newNode)`: Returns `IActionResult` **Adds a new node to the workflow.**
  - Route: `POST api/workflow/node/add`
- `UpdateNode(EditNodeViewModel updatedNode)`: Returns `IActionResult` **Updates an existing node in the workflow.**
  - Route: `PUT api/workflow/node/update`
- `DeleteNode(string nodeId)`: Returns `IActionResult` **Deletes a node from the workflow.**
  - Route: `DELETE api/workflow/node/delete/{nodeId}`
- `SaveWorkflow()`: Returns `IActionResult` **Saves the current workflow.**
  - Route: `POST api/workflow/save`
- `StartWorkflow()`: Returns `IActionResult` **Starts the workflow by redirecting to the start node.**
  - Route: `GET api/workflow/start`
- `SayThanks()`: Returns `IActionResult` **Returns a thank-you message for completing the workflow.**
  - Route: `GET api/workflow/end`

---
## Class: [ApplicationStatus](Application/Diagnostics/ApplicationStatus.cs)
Namespace: `Global`

**ApplicationStatus**

### Public Properties:
- `BuildDate`: `DateTime` ****
- `BuildVersion`: `BuildVersion` **BuildVersion**
- `Features`: `Dictionary<string, string>` **Features**
- `Messages`: `List<string>` **Messages**
- `Region`: `string?` **Region**
- `Status`: `ServiceStatus` **Status**
- `Tests`: `Dictionary<string, string>` **Tests**

### Private Methods:
- `GetBuildDate(Assembly assembly)`: Returns `DateTime` ****

---
## Class: [ConcurrentDictionaryService](ConversationDomain/ConcurrentDictionaryServiceT.cs)
Namespace: `Global`

### Public Methods:
- `Clear()`: Returns `void` **Clears all entries in the dictionary.**
- `ContainsKey(string key)`: Returns `bool` **Checks if the dictionary contains a specified key.**
- `Delete(string key)`: Returns `bool` **Deletes a value by key.**
- `GetAllKeys()`: Returns `IEnumerable<string>` **Gets all keys currently stored in the dictionary.**
- `GetOrAdd(string key, Func<string, T> valueFactory)`: Returns `T` **Gets the value associated with the specified key, or adds a new value created by the provided factory if the key does not exist.**
- `Lookup(string key)`: Returns `T?` **Looks up a value by key.**
- `Save(string key, T value)`: Returns `void` **Saves or updates a value associated with a key in the dictionary.**

---
## Class: [BuildVersion](Application/Diagnostics/BuildVersion.cs)
Namespace: `Global`

**Build Version**

### Public Properties:
- `MajorVersion`: `int` **Major Version**
- `MinorVersion`: `int` **Minor Version**
- `Build`: `int` **Build**
- `Revision`: `int` **Revision**

### Public Methods:
- `ToString()`: Returns `string` **Override the To String Function to Format Version**

---
## Class: [ConversationsController](Controllers/Api/ConversationController.cs)
Namespace: `Global`

- Inherits: `ControllerBase`

- Implements: `ControllerBase`

- Route Prefix: `api/conversations`

### Public Methods:
- `GetConversation(string conversationId)`: Returns `IActionResult` ****
  - Route: `GET api/conversations/{conversationId}`
- `StartConversation()`: Returns `IActionResult` ****
  - Route: `POST api/conversations`
- `SendMessage(string conversationId, string message, CancellationToken cancellationToken)`: Returns `Task<IActionResult>` ****
  - Route: `POST api/conversations/{conversationId}/sendMessage`
- `UserRespond(string conversationId, string userResponse, CancellationToken cancellationToken)`: Returns `Task<IActionResult>` ****
  - Route: `POST api/conversations/{conversationId}/respond`

### Private Methods:
- `CreateHateoasResponse(Conversation conversation, (MessageType messageType, object messageData)? actionData)`: Returns `object` ****
  - Route: `GET api/conversations`

---
## Class: [ChatService](ConversationDomain/ChatService.cs)
Namespace: `Global`

- Inherits: `IChatService`

- Implements: `IChatService`

### Public Methods:
- `GenerateBotResponse(ChatHistory chatHistory)`: Returns `Task<string>` ****
- `EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)`: Returns `Task` ****

---
## Class: [FlexibleStringConverter](WorkflowDomain/FlexibleStringConverter.cs)
Namespace: `Global`

**Custom JSON converter to handle flexible deserialization of string properties that may appear as numbers in JSON.**

- Inherits: `JsonConverter<string>`

### Public Methods:
- `Read(Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)`: Returns `string` ****
- `Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)`: Returns `void` ****

---
## Class: [WorkflowOptions](WorkflowDomain/WorkflowOptions.cs)
Namespace: `Global`

**Options class for configuring workflow paths.**

### Public Properties:
- `FilePath`: `string` ****
- `DirectoryPath`: `string` ****

---
## Class: [ErrorViewModel](Application/ErrorViewModel.cs)
Namespace: `Global`

### Public Properties:
- `RequestId`: `string?` ****
- `ShowRequestId`: `bool` ****

---
## Class: [WorkflowAdminController](Controllers/WorkflowAdminController.cs)
Namespace: `Global`

- Inherits: `Controller`

- Implements: `Controller`

### Public Methods:
- `ExportJson(string fileName)`: Returns `IActionResult` ****
- `AddNode(string fileName)`: Returns `IActionResult` ****
- `AddNode(EditNodeViewModel newNode)`: Returns `IActionResult` ****
- `DeleteNode(string id, string fileName)`: Returns `IActionResult` ****
- `Details(string fileName)`: Returns `IActionResult` ****
- `EditNode(string id, string fileName)`: Returns `IActionResult` ****
- `EditNode(EditNodeViewModel updatedNode)`: Returns `IActionResult` ****
- `Flowchart(string fileName)`: Returns `IActionResult` ****
- `GetWorkflowData(string fileName)`: Returns `IActionResult` ****
- `Index()`: Returns `IActionResult` ****
- `Tree(string fileName)`: Returns `IActionResult` ****
- `Workflows()`: Returns `IActionResult` ****

---
## Class: [Conversation](ConversationDomain/Conversation.cs)
Namespace: `Global`

### Public Properties:
- `ChatHistory`: `List<ChatEntry>` ****
- `ConversationId`: `string` ****
- `CurrentNodeId`: `string` ****
- `PromptName`: `string` ****
- `StartDate`: `DateTime` ****
- `UserName`: `string` ****
- `Workflow`: `Workflow` ****

---
## Class: [HomeController](Controllers/HomeController.cs)
Namespace: `Global`

- Inherits: `Controller`

- Implements: `Controller`

### Public Methods:
- `Index()`: Returns `IActionResult` ****
- `Chat()`: Returns `IActionResult` ****
- `Start()`: Returns `Task<IActionResult>` ****
- `Step(string nextLink)`: Returns `Task<IActionResult>` ****

---
## Class: [EditNodeViewModel](WorkflowDomain/EditNodeViewModel.cs)
Namespace: `Global`

- Inherits: `Node`

- Implements: `Node`

### Public Properties:
- `FileName`: `string` ****

### Public Methods:
- `UpdateNode(Node updateNode)`: Returns `Node` ****
- `GetNode()`: Returns `Node` ****

---
## Class: [Workflow](WorkflowDomain/Workflow.cs)
Namespace: `Global`

### Public Properties:
- `Nodes`: `List<Node>` ****
- `StartNode`: `string` ****
- `WorkflowId`: `string` ****
- `WorkFlowName`: `string` ****
- `WorkFlowFileName`: `string?` ****

### Private Methods:
- `ToJson()`: Returns `string` ****

---
## Class: [Node](WorkflowDomain/Node.cs)
Namespace: `Global`

### Public Properties:
- `Answers`: `List<Answer>` ****
- `Id`: `string` ****
- `QuestionType`: `QuestionType` ****
- `Question`: `string` ****

---
## Class: [Answer](WorkflowDomain/Answer.cs)
Namespace: `Global`

### Public Properties:
- `NextNode`: `string` ****
- `Response`: `string` ****
- `SystemPrompt`: `string` ****

---
## Class: [ChatEntry](ConversationDomain/ChatEntry.cs)
Namespace: `Global`

### Public Properties:
- `BotResponse`: `string` ****
- `Timestamp`: `DateTime` ****
- `User`: `string` ****
- `UserMessage`: `string` ****

---
## Class: [AnswerOption](WorkflowDomain/AnswerOption.cs)
Namespace: `Global`

### Public Properties:
- `Link`: `string` ****
- `Response`: `string` ****

---
## Class: [PromptSparkHub](ConversationDomain/PromptSparkHub.cs)
Namespace: `Global`

- Inherits: `Hub`

- Implements: `Hub`

### Public Methods:
- `SendMessage(string conversationId, string message)`: Returns `Task` ****
- `SetUserName(string conversationId, string userName, string workflowName)`: Returns `Task` ****

---
## Class: [OptionResponse](WorkflowDomain/OptionResponse.cs)
Namespace: `Global`

### Public Properties:
- `Response`: `string` ****

---
## Class: [ProgressResult](ConversationDomain/ProgressResult.cs)
Namespace: `Global`

### Public Properties:
- `IsError`: `bool` ****
- `AdaptiveCardJson`: `string?` ****
- `ErrorMessage`: `string?` ****

### Public Methods:
- `Success(string adaptiveCardJson)`: Returns `ProgressResult` ****
- `Error(string errorMessage)`: Returns `ProgressResult` ****
- `NoAdaptiveCard()`: Returns `ProgressResult` ****

---
## Class: [WorkflowNodeResponse](WorkflowDomain/WorkflowNodeResponse.cs)
Namespace: `Global`

### Public Properties:
- `Answers`: `List<AnswerOption>` ****
- `Question`: `string` ****

---
## Class: [ApiBaseController](Controllers/Api/ApiBaseController.cs)
Namespace: `Global`

- Inherits: `ControllerBase`

- Implements: `ControllerBase`

- Route Prefix: `api/apibase`

---
