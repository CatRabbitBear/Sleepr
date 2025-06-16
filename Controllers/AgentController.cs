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
    private readonly IAgentRunner _agentRunner;

    public AgentController(IAgentRunner agentRunner)
    {
        _agentRunner = agentRunner;
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
            var result = await _agentRunner.RunTaskAsync(req.History);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
