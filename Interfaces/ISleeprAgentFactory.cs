using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Sleepr.Interfaces;

public interface ISleeprAgentFactory
{
    Task<ISleeprAgent> CreateOrchestratorAgentAsync(ChatHistory history);
    Task<ISleeprAgent> CreateTaskAgentAsync(List<string> selectedPlugins, ChatHistory history);
}
