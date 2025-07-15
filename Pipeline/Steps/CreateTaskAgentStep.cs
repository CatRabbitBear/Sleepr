using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Sleepr.Interfaces;
using Sleepr.Pipeline.Interfaces;
using System.Linq;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Creates the task runner agent with the selected plugins if it doesn't already exist.
/// </summary>
public class CreateTaskAgentStep : IAgentPipelineStep
{
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly string _path;
    private readonly string _agentKey;

    public CreateTaskAgentStep(
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        string path = "task-runner",
        string agentKey = "task-agent")
    {
        _promptLoader = promptLoader;
        _templateFactory = templateFactory;
        _path = path;
        _agentKey = agentKey;
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context.Agents.ContainsKey(_agentKey))
        {
            return;
        }

        var config = await _promptLoader.LoadAsync(_path);
        var clonedKernel = context.Kernel.Clone();
        var pluginIds = new List<string>();

        foreach (var plugin in context.SelectedPlugins)
        {
            try
            {
                var manifest = context.PluginManager.GetManifestByName(plugin);
                var client = await context.PluginManager.AcquireClientAsync(manifest.Id);
                var tools = await client.ListToolsAsync();
#pragma warning disable SKEXP0001
                clonedKernel.Plugins.AddFromFunctions(manifest.Id, tools.Select(t => t.AsKernelFunction()));
#pragma warning restore SKEXP0001
                pluginIds.Add(manifest.Id);
            }
            catch
            {
                // Ignore plugins that fail to load for simplicity
            }
        }

#pragma warning disable SKEXP0001
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            Kernel = clonedKernel,
            Arguments = new KernelArguments(new PromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            })
        };
#pragma warning restore SKEXP0001
        context.Agents[_agentKey] = new AgentContext(agent, pluginIds);
    }
}
