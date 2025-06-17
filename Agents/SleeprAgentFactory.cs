using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;

namespace Sleepr.Agents;

class SleeprAgentFactory : ISleeprAgentFactory
{
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly IMcpPluginManager _pluginManager;
    private readonly Kernel _kernel;

    public SleeprAgentFactory(
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        IMcpPluginManager pluginManager,
        Kernel kernel)
    {
        _promptLoader = promptLoader;
        _templateFactory = templateFactory;
        _pluginManager = pluginManager;
        _kernel = kernel;
    }

    // The core agents are agnostic about their threads, no history needed here.
    public async Task<ChatCompletionAgent> CreateOrchestratorAgentAsync(string path = "orchestrator")
    {
        var config = await _promptLoader.LoadAsync(path);
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            // Clone so we can use different combinations of plugins for different agents
            Kernel = _kernel.Clone(),
        };
        return agent;
    }

    public async Task<ChatCompletionAgent> CreateTaskAgentAsync(string path, List<string> selectedPlugins)
    {
        throw new NotImplementedException("Task agent creation is not implemented yet.");
    }
}