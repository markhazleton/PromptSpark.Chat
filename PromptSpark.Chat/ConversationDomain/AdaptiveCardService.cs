using PromptSpark.Chat.WorkflowDomain;
using System.Text.Json;

namespace PromptSpark.Chat.ConversationDomain;

public class AdaptiveCardService
{
    public string GetAdaptiveCardForNode(Node currentNode)
    {
        return currentNode.QuestionType switch
        {
            QuestionType.Options => GetCardWithAnswers(currentNode),
            QuestionType.OptionsWithText => GetCardWithAnswersAndText(currentNode),
            QuestionType.Message => GetMessageWriteCard(),
            _ => GetCardWithText(currentNode),
        };
    }

    public string GetCardWithAnswers(Node currentNode)
    {
        var adaptiveCard = new Dictionary<string, object>
        {
            { "type", "AdaptiveCard" },
            { "version", "1.6" },
            { "body", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "type", "TextBlock" },
                        { "text", currentNode?.Question ?? "No question provided." },
                        { "wrap", true },
                        { "size", "Medium" },
                        { "weight", "Bolder" }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "TextBlock" },
                        { "text", "Select an option below:" },
                        { "wrap", true },
                        { "separator", true }
                    }
                }
            },
            { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" }
        };

        if (currentNode?.Answers != null)
        {
            var body = adaptiveCard["body"] as List<object>;
            foreach (var answer in currentNode.Answers)
            {
                body?.Add(new Dictionary<string, object>
                {
                    { "type", "ActionSet" },
                    { "actions", new object[]
                        {
                            new Dictionary<string, object>
                            {
                                { "type", "Action.Submit" },
                                { "title", answer.Response },
                                { "data", new { option = answer.Response } }
                            }
                        }
                    }
                });
            }
        }

        return JsonSerializer.Serialize(adaptiveCard);
    }

    public string GetCardWithText(Node currentNode)
    {
        var adaptiveCard = new Dictionary<string, object>
        {
            { "type", "AdaptiveCard" },
            { "version", "1.6" },
            { "body", new object[]
                {
                    new Dictionary<string, object>
                    {
                        { "type", "Input.Text" },
                        { "id", "userResponse" },
                        { "placeholder", "Type your answer here and press Enter..." },
                        { "isMultiline", false },
                        { "style", "text" },
                        { "maxLength", 100 },
                        { "inlineAction", new Dictionary<string, object>
                            {
                                { "type", "Action.Submit" },
                                { "title", "Submit" },
                                { "data", new { action = "submitText", userResponse = "${userResponse}" } }
                            }
                        }
                    }
                }
            },
            { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" }
        };

        return JsonSerializer.Serialize(adaptiveCard);
    }

    public string GetMessageWriteCard()
    {
        var adaptiveCard = new Dictionary<string, object>
        {
            { "type", "AdaptiveCard" },
            { "version", "1.6" },
            { "body", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "type", "TextBlock" },
                        { "text", "Please fill out the details below:" },
                        { "wrap", true },
                        { "size", "Medium" },
                        { "weight", "Bolder" }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "Input.Text" },
                        { "id", "title" },
                        { "placeholder", "Enter the title here" },
                        { "isMultiline", false },
                        { "style", "text" },
                        { "maxLength", 100 }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "Input.Text" },
                        { "id", "message" },
                        { "placeholder", "Type your message here" },
                        { "isMultiline", true },
                        { "style", "text" },
                        { "maxLength", 500 }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "Input.Text" },
                        { "id", "attachments" },
                        { "placeholder", "Add any attachments or URLs here" },
                        { "isMultiline", false },
                        { "style", "text" },
                        { "maxLength", 200 }
                    }
                }
            },
            { "actions", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "type", "Action.Submit" },
                        { "title", "Submit" },
                        { "data", new { action = "submitMessageForm" } }
                    }
                }
            },
            { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" }
        };

        return JsonSerializer.Serialize(adaptiveCard);
    }

    public string GetCardWithAnswersAndText(Node currentNode)
    {
        var adaptiveCard = new Dictionary<string, object>
        {
            { "type", "AdaptiveCard" },
            { "version", "1.6" },
            { "body", new object[]
                {
                    new Dictionary<string, object>
                    {
                        { "type", "TextBlock" },
                        { "text", currentNode?.Question ?? "No question provided." },
                        { "wrap", true },
                        { "size", "Medium" },
                        { "weight", "Bolder" }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "TextBlock" },
                        { "text", "Select an option below or type your response:" },
                        { "wrap", true },
                        { "separator", true }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "ActionSet" },
                        { "actions", currentNode?.Answers?.Select(answer => new Dictionary<string, object>
                            {
                                { "type", "Action.Submit" },
                                { "title", answer.Response },
                                { "data", new { option = answer.Response } }
                            }).ToArray() ?? Array.Empty<object>()
                        }
                    },
                    new Dictionary<string, object>
                    {
                        { "type", "Input.Text" },
                        { "id", "userResponse" },
                        { "placeholder", "Type your answer here and press Enter..." },
                        { "isMultiline", false },
                        { "style", "text" },
                        { "maxLength", 100 },
                        { "inlineAction", new Dictionary<string, object>
                            {
                                { "type", "Action.Submit" },
                                { "title", "Submit" },
                                { "data", new { action = "submitText", userResponse = "${userResponse}" } }
                            }
                        }
                    }
                }
            },
            { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" }
        };

        return JsonSerializer.Serialize(adaptiveCard);
    }
}
