namespace PromptSpark.Chat.Application;
/// <summary>
/// 
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
