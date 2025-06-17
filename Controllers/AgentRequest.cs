namespace Sleepr.Controllers;


public enum MessageType
{
    System,
    User,
    Assistant
}

public class AgentRequest
{
    public List<AgentRequestItem> History { get; set; } = new List<AgentRequestItem>();
    // you can add more fields as needed
}

public class AgentRequestItem
{
    public MessageType Role { get; set; }
    public string Content { get; set; } = default!;
}

public class AgentResponse
{
    public string Result { get; set; } = default!;
    public string? FilePath { get; set; }
}
