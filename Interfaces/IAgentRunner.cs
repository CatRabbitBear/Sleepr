﻿using Sleepr.Controllers;

namespace Sleepr.Interfaces;

public interface IAgentRunner
{
    Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history);
}
