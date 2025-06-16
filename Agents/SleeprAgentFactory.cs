using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;

namespace Sleepr.Agents;

class SleeprAgentFactory : ISleeprAgentFactory
{
    private readonly Kernel _kernel;
    private readonly IMcpPluginManager _pluginManager;
    public SleeprAgentFactory(Kernel kernel, IMcpPluginManager pluginManager)
    {
        _kernel = kernel;
        _pluginManager = pluginManager;
    }
    public async Task<ISleeprAgent> CreateOrchestratorAgentAsync(ChatHistory history)
    {
        throw new NotImplementedException("Orchestrator agent creation is not implemented yet.");
    }

    public async Task<ISleeprAgent> CreateTaskAgentAsync(List<string> selectedPlugins, ChatHistory history)
    {
        throw new NotImplementedException("Task agent creation is not implemented yet.");
    }
}