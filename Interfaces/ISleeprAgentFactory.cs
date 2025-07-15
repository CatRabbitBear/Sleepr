using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Pipeline;

namespace Sleepr.Interfaces;

public interface ISleeprAgentFactory
{
    Task<AgentContext> CreateOrchestratorAgentAsync(string path);
    Task<AgentContext> CreateTaskAgentAsync(string path, List<string> selectedPlugins);
}
