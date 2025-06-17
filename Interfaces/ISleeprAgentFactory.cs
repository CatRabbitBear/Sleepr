using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Sleepr.Interfaces;

public interface ISleeprAgentFactory
{
    Task<ChatCompletionAgent> CreateOrchestratorAgentAsync(string path);
    Task<ChatCompletionAgent> CreateTaskAgentAsync(string path, List<string> selectedPlugins);
}
