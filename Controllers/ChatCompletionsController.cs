using Microsoft.AspNetCore.Mvc;
using Sleepr.Interfaces;

namespace Sleepr.Controllers;

[ApiController]
[Route("api/chat-completions")]
public class ChatCompletionsController : ControllerBase
{
    private readonly ILogger<ChatCompletionsController> _logger;
    private readonly IChatCompletionsRunner _chatCompletionsRunner;
    public ChatCompletionsController(ILogger<ChatCompletionsController> logger, IChatCompletionsRunner chatCompletionsRunner)
    {
        _logger = logger;
        _chatCompletionsRunner = chatCompletionsRunner;
    }

    [HttpPost("run-task")]
    public async Task<ActionResult<AgentResponse>> RunTask([FromBody] AgentRequest req)
    {
        try
        {
            var result = await _chatCompletionsRunner.RunTaskAsync(req.History);
            _logger.LogInformation("Task completed successfully by chat completions runner. Items returned : {Count}", req.History.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running task in chat completions runner.");
            return BadRequest(new { error = ex.Message });
        }
    }

}
