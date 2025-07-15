using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Sleepr.Interfaces;
using Sleepr.Pipeline;
using Sleepr.Services;

namespace Sleepr.Agents;

class SleeprAgentFactory : ISleeprAgentFactory
{
    private readonly ILogger<SleeprAgentFactory> _logger;
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly McpPluginManager _pluginManager;
    private readonly Kernel _kernel;

    public SleeprAgentFactory(
        ILogger<SleeprAgentFactory> logger,
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        McpPluginManager pluginManager,
        Kernel kernel)
    {
        _logger = logger;
        _promptLoader = promptLoader;
        _templateFactory = templateFactory;
        _pluginManager = pluginManager;
        _kernel = kernel;
    }

    // The core agents are agnostic about their threads, no history needed here.
    public async Task<AgentContext> CreateOrchestratorAgentAsync(string path = "orchestrator")
    {
        var config = await _promptLoader.LoadAsync(path);
        _logger.LogInformation("Creating Orchestrator Agent with config name: {Config}", config.Name);
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            // Clone so we can use different combinations of plugins for different agents
            Kernel = _kernel.Clone(),
        };
        return new AgentContext(agent);
    }

    public async Task<AgentContext> CreateTaskAgentAsync(string path, List<string> selectedPlugins)
    {
        var config = await _promptLoader.LoadAsync(path);
        var clonedKernel = _kernel.Clone();
        var pluginIds = new List<string>();
        foreach (var plugin in selectedPlugins)
        {
            try
            {
                // Add the plugin to the cloned kernel
                var manifest = _pluginManager.GetManifestByName(plugin);
                var client = await _pluginManager.AcquireClientAsync(manifest.Id);
                var tools = await client.ListToolsAsync();
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                clonedKernel.Plugins.AddFromFunctions(manifest.Id, tools.Select(aiFunction => aiFunction.AsKernelFunction()));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                pluginIds.Add(manifest.Id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Plugin not found: {Plugin}", plugin);
                // Handle the case where the plugin is not found
                // You might want to throw an exception or log an error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add plugin: {Plugin}", plugin);
                // Handle other exceptions that may occur
            }

        }
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            Kernel = clonedKernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) })
        };
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        return new AgentContext(agent, pluginIds);

    }
}