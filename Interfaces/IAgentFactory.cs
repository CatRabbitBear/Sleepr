using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using Sleepr.Pipeline;

namespace Sleepr.Interfaces;

public interface IAgentFactory
{
    Task<AgentContext> CreateOrchestratorAgentAsync(string promptPath);
    Task<AgentContext> CreateTaskAgentAsync(string promptPath, List<string> selectedPlugins);
}