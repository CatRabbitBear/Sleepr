# Agent Pipeline Roadmap

The initial pipeline refactor is complete. Each agent is now created by `AgentFactory` and paired with its own `IAgentPipeline`. Pipelines consist of small steps that operate on a shared `PipelineContext`. `SleeprAgentRunner` demonstrates how multiple agents can be chained together using this model.

## Current Capabilities
- Orchestrator and task agents are built via `AgentFactory`.
- Pipelines run without explicit `init`/`cleanup` phases; each step focuses on a single unit of work.
- Plugin selection is delegated to the orchestrator agent and injected when creating the task agent.

## Next Steps
1. **Overseer agent** – manage the execution of other agents and pipelines. This agent would allow nested pipelines and provide a place for global reasoning about goals.
2. **Agent collaboration** – enable pipelines that pass control between multiple agents, sharing context or results.
3. **Human in the loop** – support pausing a pipeline to ask the user for confirmation before proceeding. A controller could surface intermediate output and await user input.
4. **Logging and telemetry** – richer diagnostics around each pipeline step to aid debugging of complex workflows.

The long term goal is to make pipelines composable so higher level agents (or humans) can orchestrate lower level behaviours easily.
