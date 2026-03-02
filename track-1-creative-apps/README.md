# Code Intelligence MCP Server

**Track**: Creative Apps - Build with GitHub Copilot

An MCP (Model Context Protocol) server that gives GitHub Copilot superpowers for code understanding. Ask Copilot to explain any code, convert between languages, solve problems with code, analyze UI mockups, visualize architectures as Mermaid diagrams, and interpret OpenTelemetry traces — all from Copilot Chat.

---

## What It Does

Code Intelligence MCP exposes **6 AI-powered tools** to any MCP-compatible client (GitHub Copilot, VS Code, Copilot CLI):

| Tool | What It Does |
|------|-------------|
| **ExplainCode** | Analyzes source code: algorithm identification, Big-O complexity, bug detection, improvement suggestions |
| **ConvertCode** | Converts code between any two languages with idiomatic target-language patterns |
| **SolveWithCode** | Turns natural language problem descriptions into parameterized, runnable code |
| **AnalyzeVisual** | Reviews text descriptions of UI designs (ASCII mockups, design specs) for accessibility, layout, and implementation guidance |
| **GenerateArchitectureDiagram** | Scans a codebase directory and generates interactive Mermaid architecture diagrams with live editor links |
| **ExplainTrace** | Transforms raw OpenTelemetry JSON spans into human-readable narratives |

### The Creative Angle

This isn't just another chatbot — it's **a tool that teaches Copilot to see code the way a senior engineer does**. The architecture diagram tool scans real files, budgets by character count, generates Mermaid diagrams, detects and fixes subgraph name collisions, and produces clickable [mermaid.live](https://mermaid.live) links. The OTel tool turns invisible infrastructure data into stories anyone can act on.

**Meta twist**: Every tool call is itself instrumented with OpenTelemetry — the tool that explains code also explains itself.

---

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A [GitHub personal access token](https://github.com/settings/tokens) (for GitHub Models free tier — no Azure spend required)

### Setup

```bash
# 1. Clone and navigate
git clone https://github.com/AncpLua/agentsleague-starter-kits.git
cd agentsleague-starter-kits/track-1-creative-apps/src/CodeIntelligenceMcp

# 2. Set your GitHub token
cp .env.example .env
# Edit .env and add your GitHub PAT
export GITHUB_TOKEN="your-github-token-here"

# 3. Build and run
dotnet build
dotnet run
```

### Connect to GitHub Copilot (VS Code)

Add this to your VS Code `settings.json` or your project's `.vscode/mcp.json`:

```json
{
  "mcp": {
    "servers": {
      "code-intelligence": {
        "type": "stdio",
        "command": "dotnet",
        "args": ["run", "--project", "path/to/CodeIntelligenceMcp"],
        "env": {
          "GITHUB_TOKEN": "${env:GITHUB_TOKEN}"
        }
      }
    }
  }
}
```

Then open Copilot Chat and ask:
- *"Explain this Go code and identify the algorithm"*
- *"Convert this Python to idiomatic C#"*
- *"Generate an architecture diagram for this project"*
- *"Why is this OTel trace slow?"*

---

## Architecture

```
Copilot Chat (VS Code / CLI)
    -> discovers MCP server via .mcp/server.json
    -> MCP server started (stdio transport)
    -> tool call routed via JSON-RPC
    -> tool method executes with IChatClient (GitHub Models free tier)
    -> structured result returned to Copilot
    -> Copilot reasons over the result and presents to user
```

### Project Structure

```
CodeIntelligenceMcp/
├── Program.cs                    # Entry point: DI, MCP server, OTel setup
├── Tools/
│   ├── CodeIntelligenceTools.cs  # ExplainCode, ConvertCode, SolveWithCode, AnalyzeVisual
│   ├── ArchitectureTools.cs      # GenerateArchitectureDiagram
│   └── OtelInsightTools.cs       # ExplainTrace
├── Pipeline/
│   ├── FileScanner.cs            # Glob-based file discovery
│   ├── TokenBudgeter.cs          # Character budget allocation for LLM context
│   ├── PromptBuilder.cs          # LLM prompt construction
│   └── SourceFile.cs             # Source file record
├── Mermaid/
│   ├── CycleDetector.cs          # Detects and fixes subgraph name collisions
│   ├── LinkGenerator.cs          # mermaid.live URL generator
│   └── Serializer.cs             # Zlib compression for mermaid.live URL format
├── Llm/
│   └── DiagramGenerator.cs       # IChatClient wrapper for diagram generation
├── Output/
│   └── DiagramFormatter.cs       # Mermaid extraction and formatting
├── Instrumentation/
│   └── McpActivitySource.cs      # OTel self-instrumentation
└── .mcp/
    └── server.json               # MCP server discovery config
```

### Key Design Decisions

- **GitHub Models free tier** — Zero Azure spend. Any developer with a GitHub token can run this.
- **`IChatClient` abstraction** — Model-swappable at the DI level via `Microsoft.Extensions.AI`.
- **OTel self-instrumentation** — Every tool call emits spans with `gen_ai.operation.name`, `gen_ai.request.model`, and `gen_ai.usage.output_tokens` attributes. Observable with any OTel-compatible backend.
- **Mermaid name-collision detection** — LLMs sometimes generate Mermaid diagrams where a subgraph and a node share the same name (causing render errors). The `CycleDetector` detects and renames these automatically.

---

## Key Technologies

- **.NET 10** — Latest runtime
- **ModelContextProtocol SDK** (`ModelContextProtocol` NuGet) — MCP server implementation
- **Microsoft.Extensions.AI** — Model-agnostic `IChatClient` abstraction
- **GitHub Models** (gpt-4o-mini) — Free-tier LLM inference
- **OpenTelemetry** — Self-instrumentation with OTLP export
- **Microsoft.Extensions.FileSystemGlobbing** — File scanning with glob patterns

---

## GitHub Copilot Usage

This project was built entirely with GitHub Copilot assistance:

- **Copilot Agent Mode** — Used to scaffold the MCP server structure, generate tool implementations, and iterate on prompt engineering
- **Copilot Chat** — Used for debugging MCP protocol issues, understanding the `ModelContextProtocol` SDK API, and designing the Mermaid serialization format
- **Copilot Inline Suggestions** — Accelerated implementation of the pipeline classes (FileScanner, TokenBudgeter, CycleDetector)
- **Copilot Plan Mode** — Used to architect the 6-tool design and plan the file scanning pipeline

The MCP server itself is designed to extend Copilot — demonstrating Copilot building a tool that makes Copilot smarter.

---

## Responsible AI

### Safety Measures

- **No code execution** — All tools perform analysis only. No user-submitted code is executed on the server.
- **Input size limits** — The `GenerateArchitectureDiagram` tool enforces glob exclusion patterns, file count limits, and a character budget to prevent resource exhaustion. Other tools delegate input handling to the LLM.
- **No data persistence** — The server is stateless. No user code, traces, or analysis results are stored.
- **Environment-based credentials** — GitHub token is read from environment variables, never hardcoded.

### Limitations

- Analysis quality depends on the underlying LLM (gpt-4o-mini). Results should be verified by a human.
- Code conversion preserves semantics on a best-effort basis — edge cases in language-specific behavior may not be caught.
- Architecture diagrams are high-level approximations, not guaranteed-complete representations.

---

## License

[MIT License](../../LICENSE)
