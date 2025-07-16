# Agent Pipeline API Guide

This document explains the current API for building and executing agent pipelines. Pipelines are composed of small `IAgentPipelineStep` implementations that operate on a mutable `PipelineContext`.

## PipelineContext

```csharp
public class PipelineContext
{
    public List<AgentRequestItem>? RequestHistory { get; }
    public List<string> SelectedPlugins { get; set; } = new();
    public ChatHistory? ChatHistory { get; set; }
    public ChatHistoryAgentThread? AgentThread { get; set; }
    public string? UserMessage { get; set; }
    public string? FinalResult { get; set; }
    public string? FilePath { get; set; }
    public List<string> Trace { get; } = new();

    public PipelineContext(List<AgentRequestItem> history)
    {
        RequestHistory = history;
        if (history.Count > 0 && history.Last().Role == MessageType.User)
        {
            UserMessage = history.Last().ToString();
        }
        AgentThread = new ChatHistoryAgentThread();
    }

    public PipelineContext(string userMessage)
    {
        UserMessage = userMessage;
        AgentThread = new ChatHistoryAgentThread();
    }
}
```

## AgentContext

Every agent is created through `AgentFactory` and returned as an `AgentContext` containing the agent and its pipeline.

```csharp
public class AgentContext
{
    public ChatCompletionAgent Agent { get; }
    public IAgentPipeline Pipeline { get; set; }
    public PipelineContext? PipelineContext { get; set; }
    public List<string> Trace { get; } = new();

    public AgentContext(ChatCompletionAgent agent, IAgentPipeline pipeline)
    {
        Agent = agent;
        Pipeline = pipeline;
    }
}
```

## Building Pipelines

Steps are added to an `AgentPipelineBuilder` in the order they should execute:

```csharp
var pipeline = new AgentPipelineBuilder()
    .Use(new LoadChatHistoryStep(loggerFactory))
    .Use(new RunTaskAgentStep(myAgent, loggerFactory))
    .Use(new SaveOutputStep(output, loggerFactory))
    .Build();
```

`AgentPipeline.RunAsync` executes each step with the provided `PipelineContext`.

## Example Usage

```csharp
var orchestratorAgent = await factory.CreateOrchestratorAgentAsync("orchestrator");
var orchestratorContext = contextFactory.Create(history);
await orchestratorAgent.Pipeline.RunAsync(orchestratorContext);

var taskAgent = await factory.CreateTaskAgentAsync("task", orchestratorContext.SelectedPlugins);
var taskContext = contextFactory.Create(history);
await taskAgent.Pipeline.RunAsync(taskContext);
```

Pipelines no longer contain explicit `init` or `cleanup` phases. Any setup should happen inside steps or when the agent is created.
