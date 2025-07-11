# Agent Pipeline Refactor Roadmap

This document outlines the high level plan for restructuring how agent requests are processed. The goal is to simplify the `AgentRunner` layer, encourage composition over inheritance and make it easy to build pipelines with only the steps required for a given task.

## Motivation
- `SleeprAgentRunner` currently orchestrates history parsing, tool selection, plugin loading, agent invocation and output persistence. All responsibilities are tightly coupled and hard to extend.
- New integrations tend to add more code inside runner classes, increasing complexity.
- We want controllers to compose pipelines from small building blocks and to reuse those blocks across different agent types.

## Design Goals
1. **Composable steps** – each operation (loading prompts, selecting plugins, running an agent, saving output, etc.) becomes a standalone step that implements a common interface.
2. **Fluent builder API** – pipelines are assembled using a fluent style allowing steps to be reused or replaced.
3. **Kernel cloning with plugin subsets** – keep the current behaviour where only a subset of plugins is added to a cloned kernel. Selection logic lives in a dedicated step.
4. **No inheritance requirements** – steps are plain classes implementing a single interface. Pipelines rely on composition of these steps.
5. **Ease of extension** – new behaviours can be introduced by writing additional steps or decorators without changing existing ones.

## Current State Summary
- Requests hit `AgentController` or `ChatCompletionsController`.
- `IAgentRunner` and `IChatCompletionsRunner` perform all orchestration internally.
- `SleeprAgentFactory` clones the kernel and injects plugins based on names returned from the orchestrator.
- Plugin discovery and acquisition live in `McpPluginManager`.

While functional, this design makes it difficult to reuse parts of the flow (for example running the orchestrator alone, or injecting plugins for a different type of agent). Error handling and logging are spread across the runners.

## Proposed Architecture
### Core Concepts
- **PipelineContext** – a data object passed between steps containing the request history, kernel, plugin manager and arbitrary metadata (selected plugin names, final result, etc.).
- **IAgentPipelineStep** – interface with a single method:
  ```csharp
  Task ExecuteAsync(PipelineContext context);
  ```
  Each step reads and modifies the context as needed.
- **AgentPipeline** – executes a list of `IAgentPipelineStep` instances sequentially.
- **AgentPipelineBuilder** – fluent API for constructing pipelines. Example:
  ```csharp
  var pipeline = new AgentPipelineBuilder()
      .Use(new LoadChatHistoryStep())
      .Use(new SelectPluginsStep())
      .Use(new LoadPluginsStep())
      .Use(new RunTaskAgentStep("task-runner"))
      .Use(new SaveOutputStep())
      .Build();
  ```

### Example Steps
- **LoadChatHistoryStep** – converts `AgentRequest` into `ChatHistory`/`AgentThread` structures.
- **SelectPluginsStep** – runs the orchestrator agent or other logic to decide which plugins to add. It populates `context.SelectedPlugins`.
- **LoadPluginsStep** – clones `context.Kernel` and adds only the plugins from `context.SelectedPlugins` using `McpPluginManager`.
- **RunTaskAgentStep** – executes a task agent using the cloned kernel.
- **SaveOutputStep** – persists the final response using an `IAgentOutput` implementation.

Steps can be replaced or extended. For instance, a custom step may log telemetry, validate output or branch to multiple agents.

### Controller Interaction
Controllers request an `IAgentPipelineBuilder` from DI, configure it and execute the resulting pipeline:
```csharp
var pipeline = pipelineBuilder
    .UseDefaultHistory()
    .UseOrchestrator()
    .UseTaskRunner()
    .Build();
var response = await pipeline.RunAsync(new PipelineContext(history));
```
`UseOrchestrator` and `UseTaskRunner` are extension methods that register standard steps.

### Retaining Plugin Subset Behaviour
`LoadPluginsStep` will clone the kernel (`context.Kernel.Clone()`) and only add plugins whose names appear in `context.SelectedPlugins`. The orchestration logic that decides these names is contained in `SelectPluginsStep` which could rely on an orchestrator agent or any other mechanism.

## Migration Plan
1. **Introduce core abstractions** (`PipelineContext`, `IAgentPipelineStep`, `AgentPipeline`, `AgentPipelineBuilder`).
2. **Refactor existing logic** from `SleeprAgentRunner` into the step classes listed above.
3. **Create extension methods** to easily register common pipelines (e.g. `builder.UseStandardAgent()`).
4. **Update controllers** to build and run pipelines using the builder instead of calling runners directly.
5. **Deprecate old runners** once the new pipeline is fully validated.

## Next Steps
- Draft the exact interfaces and builder API (see `PIPELINE_API.md`).
- Incrementally move functionality from existing runners into pipeline steps.
- Provide unit tests for each step to ensure behaviour parity with the current implementation.

