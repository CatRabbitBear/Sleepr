﻿using Microsoft.AspNetCore.Mvc;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;

namespace Sleepr.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentRunner _agentRunner;

    public AgentController(IAgentRunner agentRunner)
    {
        _agentRunner = agentRunner;
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
