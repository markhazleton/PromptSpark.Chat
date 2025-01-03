namespace PromptSpark.Chat.WorkflowDomain;

public class EditNodeViewModel : Node
{
    public EditNodeViewModel()
    {
    }
    public EditNodeViewModel(Node node, string filename)
    {
        Id = node.Id;
        Question = node.Question;
        Answers = node.Answers;
        FileName = filename;
        QuestionType = node.QuestionType;
    }
    public Node UpdateNode(Node updateNode)
    {
        updateNode.Question = Question;
        updateNode.Answers = Answers;
        updateNode.QuestionType = QuestionType;
        return updateNode;
    }
    public Node GetNode()
    {
        return new Node
        {
            Id = Id,
            Question = Question,
            Answers = Answers,
            QuestionType = QuestionType
        };
    }
    public required string FileName { get; set; }
}
