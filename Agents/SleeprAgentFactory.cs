using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Sleepr.Interfaces;
using Sleepr.Services;

namespace Sleepr.Agents;

class SleeprAgentFactory : ISleeprAgentFactory
{
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly McpPluginManager _pluginManager;
    private readonly Kernel _kernel;

    public SleeprAgentFactory(
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        McpPluginManager pluginManager,
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
        var config = await _promptLoader.LoadAsync(path);
        var clonedKernel = _kernel.Clone();
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
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"ERROR: Plugin '{plugin}' not found. {ex.Message}");
                // Handle the case where the plugin is not found
                // You might want to throw an exception or log an error
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to add plugin '{plugin}'. {ex.Message}");
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
        return agent;

    }
}