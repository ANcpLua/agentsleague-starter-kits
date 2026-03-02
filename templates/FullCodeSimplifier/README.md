# Full Code Simplifier

A production-style **Code Simplifier** sample built with:

- `GitHub.Copilot.SDK`
- `Microsoft.Agents.AI`
- `Microsoft.Agents.AI.Workflows`
- `Microsoft.Agents.AI.GitHub.Copilot`
- `Microsoft.Agents.AI.OpenAI`
- `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore`
- OpenTelemetry (`OpenTelemetry`, OTLP exporter)

It runs a two-stage simplification pipeline:

1. `CodeSimplifier` agent proposes a behavior-preserving refactor.
2. `CodeReviewer` agent validates and rewrites to the safest final version.

It supports:

- Copilot-backed agent sessions (`CopilotClient` + `SessionConfig`)
- Azure OpenAI / Responses API-backed agents
- GitHub Models-backed agents
- MCP server registration (`filesystem`, `microsoft-learn`)
- Interactive permission approvals
- Workflow orchestration (`AgentWorkflowBuilder` + `InProcessExecution`)
- AG-UI hosting endpoint (`MapAGUI`)
- OpenTelemetry tracing (`AddSource("*Microsoft.Agents.AI")`)

## Safety and Prerequisites

> Warning: Copilot sessions can execute tools and shell commands. Run this in a container or disposable environment for safety.

- .NET 10 SDK or later
- For `--provider copilot`: install `copilot` CLI and ensure it is available on `PATH`
- For `--provider azure-openai`: configure Azure endpoint and model/deployment environment variables
- For `--provider github-models`: configure GitHub token and model endpoint environment variables

## Quick Start

```bash
cd samples/FullCodeSimplifier
dotnet restore
dotnet build
```

### 1) Copilot provider (default)

```bash
dotnet run -- simplify --file Program.cs --provider copilot --enable-mcp --stream
```

Apply changes to disk:

```bash
dotnet run -- simplify --file Program.cs --provider copilot --apply
```

### 2) Azure OpenAI provider (Responses API)

Set env vars:

```bash
export AZURE_OPENAI_ENDPOINT="https://<resource>.openai.azure.com/openai/v1"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4.1"
```

Run:

```bash
dotnet run -- simplify --file Program.cs --provider azure-openai
```

### 3) GitHub Models provider

Set env vars:

```bash
export GITHUB_MODELS_TOKEN="<token>"
export GITHUB_MODELS_ENDPOINT="https://models.inference.ai.azure.com"
export GITHUB_MODELS_MODEL="gpt-4.1"
```

Run:

```bash
dotnet run -- simplify --file Program.cs --provider github-models
```

## AG-UI Hosting

```bash
dotnet run -- serve-agui --provider copilot --agui-url http://localhost:50516
```

- AG-UI endpoint: `http://localhost:50516/`
- Health endpoint: `http://localhost:50516/health`

## OpenTelemetry

Enable OTEL:

```bash
export ENABLE_OTEL=true
export OTLP_ENDPOINT=http://localhost:4317
```

The app wires tracing through both the agent pipeline (`UseOpenTelemetry`) and the OTLP exporter.

## Output Artifacts

Each run creates a report in:

- `artifacts/full-code-simplifier/report-<timestamp>.md`

The report includes:

- file metadata
- unified diff
- raw reviewer output

## Notes

- This sample intentionally avoids automatic test execution.
- For non-trivial repositories, keep `--simplification-level balanced` to reduce risky transformations.
- Use `--max-input-bytes` to cap prompt payload size for large files.
