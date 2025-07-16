# Sleepr

Sleepr is an experimental framework for orchestrating Semantic Kernel agents. Each agent is executed through a small pipeline composed of reusable steps. Pipelines pass a `PipelineContext` object between steps and agents are created via the `AgentFactory` which wires up the appropriate pipeline for that agent.

Each pipeline step receives an `ILoggerFactory` so it can create a logger instance without being resolved from the DI container. The `PipelineContext` and `AgentContext` now expose a `Trace` list for recording decision details during execution.

The project currently exposes two main runners:

- **ChatCompletionsRunner** – minimal pipeline that simply runs a chat completion and persists the result.
- **SleeprAgentRunner** – uses an orchestrator agent to decide which plugins to load before running the task agent.

## SQLite Output Store

Agent responses are stored using SQLite. Set the connection string in a `.env` file:

```bash
OUTPUT_DB_CONNECTION_STRING=Data Source=data/agent-output.db
```

The path can also be overridden in `appsettings.json` under `OutputDb:Path`.
