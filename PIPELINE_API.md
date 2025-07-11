# Agent Pipeline API Guide

This document describes the proposed fluent API for composing and executing agent pipelines after the refactor. The API focuses on flexibility and minimal boilerplate while maintaining the ability to restrict available plugins.

## Core Interfaces

```csharp
// Carries request data, cloned kernel and any intermediate state
public class PipelineContext
{
    public List<AgentRequestItem> RequestHistory { get; }
    public Kernel Kernel { get; set; }
    public McpPluginManager PluginManager { get; }
    public IList<string> SelectedPlugins { get; set; } = new List<string>();
    public string? FinalResult { get; set; }
    // other fields as needed

    public PipelineContext(List<AgentRequestItem> history,
                           Kernel kernel,
                           McpPluginManager pluginManager)
    {
        RequestHistory = history;
        Kernel = kernel;
        PluginManager = pluginManager;
    }
}

public interface IAgentPipelineStep
{
    Task ExecuteAsync(PipelineContext context);
}

public interface IAgentPipeline
{
    Task RunAsync(PipelineContext context);
}

public interface IAgentPipelineBuilder
{
    IAgentPipelineBuilder Use(IAgentPipelineStep step);
    IAgentPipeline Build();
}
```

## Building a Pipeline

Steps are registered in the order they should run. The builder returns an `IAgentPipeline` that can be executed with a `PipelineContext`.

```csharp
var pipeline = new AgentPipelineBuilder()
    .Use(new LoadChatHistoryStep())
    .Use(new SelectPluginsStep(orchestratorPath: "orchestrator"))
    .Use(new LoadPluginsStep())
    .Use(new RunTaskAgentStep("task-runner"))
    .Use(new SaveOutputStep())
    .Build();

var context = new PipelineContext(req.History, kernel, pluginManager);
await pipeline.RunAsync(context);
string? response = context.FinalResult;
```

### Extension Methods

Common sequences can be exposed via extension methods on `IAgentPipelineBuilder` so controllers stay concise:

```csharp
public static class StandardPipelineExtensions
{
    public static IAgentPipelineBuilder UseDefaultHistory(this IAgentPipelineBuilder builder)
        => builder.Use(new LoadChatHistoryStep());

    public static IAgentPipelineBuilder UseOrchestrator(this IAgentPipelineBuilder builder, string path = "orchestrator")
        => builder.Use(new SelectPluginsStep(path));

    public static IAgentPipelineBuilder UseTaskRunner(this IAgentPipelineBuilder builder, string path = "task-runner")
        => builder.Use(new RunTaskAgentStep(path));
}
```

Controllers can then compose a pipeline using these helpers:

```csharp
var pipeline = pipelineBuilder
    .UseDefaultHistory()
    .UseOrchestrator()
    .Use(new LoadPluginsStep())
    .UseTaskRunner()
    .Use(new SaveOutputStep())
    .Build();

await pipeline.RunAsync(new PipelineContext(req.History, kernel, pluginManager));
```

## Loading Plugins

`LoadPluginsStep` is responsible for cloning the kernel and injecting only the requested plugins:

```csharp
public class LoadPluginsStep : IAgentPipelineStep
{
    public async Task ExecuteAsync(PipelineContext context)
    {
        var clone = context.Kernel.Clone();
        foreach (var pluginName in context.SelectedPlugins)
        {
            var manifest = context.PluginManager.GetManifestByName(pluginName);
            var client = await context.PluginManager.AcquireClientAsync(manifest.Id);
            var tools = await client.ListToolsAsync();
            clone.Plugins.AddFromFunctions(manifest.Id, tools.Select(t => t.AsKernelFunction()));
        }
        context.Kernel = clone;
    }
}
```

This keeps the plugin subset behaviour intact and isolates it from the rest of the pipeline.

## Running from Controllers

Controllers obtain an `IAgentPipelineBuilder` and configure the pipeline per request.

```csharp
[HttpPost("run-task")]
public async Task<ActionResult<AgentResponse>> RunTask([FromBody] AgentRequest req)
{
    var context = new PipelineContext(req.History, _kernel, _pluginManager);
    var pipeline = _pipelineBuilder
        .UseDefaultHistory()
        .UseOrchestrator()
        .Use(new LoadPluginsStep())
        .UseTaskRunner()
        .Use(new SaveOutputStep())
        .Build();

    await pipeline.RunAsync(context);
    return Ok(new AgentResponse { Result = context.FinalResult ?? string.Empty });
}
```

Steps can easily be swapped or removed. Additional pipelines might include alternate plugin selectors, additional pre/post-processing or specialised agent types.


