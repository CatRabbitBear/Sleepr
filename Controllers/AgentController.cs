using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;

namespace Sleepr.Controllers;

public enum MessageType
{
    System,
    User,
    Assistant
}

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly IAgentOutput _outputManager;

    public AgentController(Kernel kernel, IAgentOutput outputManager)
    {
        _kernel = kernel;
        _outputManager = outputManager;
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

    [HttpPost("run-task")]
    public async Task<ActionResult<AgentResponse>> RunTask([FromBody] AgentRequest req)
    {
        try
        {
            ChatHistory chatHistory = new ChatHistory();
            foreach (var item in req.History)
            {
                switch (item.Role)
                {
                    case MessageType.System:
                        chatHistory.AddSystemMessage(item.Content);
                        break;
                    case MessageType.User:
                        chatHistory.AddUserMessage(item.Content);
                        break;
                    case MessageType.Assistant:
                        chatHistory.AddAssistantMessage(item.Content);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item.Role), "Invalid message type.");
                }
            }

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatService.GetChatMessageContentAsync(chatHistory);

            if (result.Content != null)
            {
                // Save the result to a file if needed
                var filePath = await _outputManager.SaveAsync(result.Content);
                return Ok(new AgentResponse
                {
                    Result = result.Content,
                    FilePath = filePath
                });
            }
            else
            {
                return BadRequest(new { error = "No result returned from chat service." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during chat completion: {ex.Message}");
            return BadRequest(new { error = "An error occurred while processing the request." });
        }
    }
}
