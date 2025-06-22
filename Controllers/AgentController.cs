using Microsoft.AspNetCore.Mvc;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;

namespace Sleepr.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly ILogger<AgentController> _logger;
    private readonly IAgentRunner _agentRunner;

    public AgentController(ILogger<AgentController> logger,  IAgentRunner agentRunner)
    {
        _logger = logger;
        _agentRunner = agentRunner;
    }

    [HttpPost("run-task")]
    public async Task<ActionResult<AgentResponse>> RunTask([FromBody] AgentRequest req)
    {
        try
        {
            var result = await _agentRunner.RunTaskAsync(req.History);
            _logger.LogInformation("Task run successfully with result: {Result}", result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running task");
            return BadRequest(new { error = ex.Message });
        }
    }
}
