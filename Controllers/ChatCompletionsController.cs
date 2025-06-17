using Microsoft.AspNetCore.Mvc;
using Sleepr.Interfaces;

namespace Sleepr.Controllers;

[ApiController]
[Route("api/chat-completions")]
public class ChatCompletionsController : ControllerBase
{
    private readonly IChatCompletionsRunner _chatCompletionsRunner;
    public ChatCompletionsController(IChatCompletionsRunner chatCompletionsRunner)
    {
        _chatCompletionsRunner = chatCompletionsRunner;
    }

    [HttpPost("run-task")]
    public async Task<ActionResult<AgentResponse>> RunTask([FromBody] AgentRequest req)
    {
        try
        {
            var result = await _chatCompletionsRunner.RunTaskAsync(req.History);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}
