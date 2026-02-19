# Agents League Starter Kits

Opinionated starter kits for the **Microsoft Agents League Hackathon** — 3 competition tracks, 4 .NET AI templates, one repo.

Built with the latest **v10.x** templates from `dotnet/extensions`, targeting **.NET 10** and **C# 14**.

## What's Inside

```
.
├── track-1-creative-apps/       # Creative Apps with GitHub Copilot
│   └── README.md                # Official track rules + .NET template guide
├── track-2-reasoning-agents/    # Reasoning Agents with Microsoft Foundry
│   ├── README.md                # Official track rules + C# development path
│   └── reasoning-agents-architecture.png
├── track-3-enterprise-agents/   # Enterprise Agents with M365 Agents Toolkit
│   └── README.md                # Official track rules + CEA template guide
├── templates/                   # Scaffolded .NET AI project templates (v10.x)
│   ├── McpServerApp/            # MCP Server (ModelContextProtocol 0.7.0)
│   ├── AiChatWeb/               # AI Chat Web App (M.E.AI 10.3.0, Blazor, RAG)
│   ├── AiAgentWebApi/           # AI Agent Web API (Agent Framework 1.0.0)
│   └── CustomEngineAgent/       # M365 Custom Engine Agent (ATK)
├── FILE-MAP.md                  # Every file explained — what it does, which track
└── README.md                    # You are here
```

## Quick Start

### 1. Install the templates

```bash
dotnet new install Microsoft.Extensions.AI.Templates          # aichatweb
dotnet new install Microsoft.McpServer.ProjectTemplates       # mcpserver
dotnet new install Microsoft.Agents.AI.ProjectTemplates       # aiagent-webapi
```

### 2. Pick your track

| Track | Start with | Why |
|-------|-----------|-----|
| **Creative Apps** | `mcpserver` + `aichatweb` | MCP tools for Copilot + standalone chat UI |
| **Reasoning Agents** | `aiagent-webapi` + `mcpserver` | Multi-agent orchestrator + tool servers |
| **Enterprise Agents** | `aiagent-webapi` + `mcpserver` + `cea` | CEA backend + MCP integration (8 bonus pts) |

### 3. Scaffold your project

```bash
dotnet new mcpserver -n MyProject        # MCP Server
dotnet new aichatweb -n MyProject        # AI Chat Web App
dotnet new aiagent-webapi -n MyProject   # AI Agent Web API
dotnet new cea -n MyProject              # M365 Custom Engine Agent
```

### 4. Configure AI provider

All templates default to **GitHub Models** (free tier). Set your token:

```bash
cd MyProject
dotnet user-secrets set "GITHUB_TOKEN" "your-github-token"
# or for aichatweb:
dotnet user-secrets set "GitHubModels:Token" "your-github-token"
```

## Template Versions

| Template | NuGet Package | Version | Key Dependencies |
|----------|--------------|---------|-----------------|
| `mcpserver` | `Microsoft.McpServer.ProjectTemplates` | `0.7.0-preview.1.26109.11` | ModelContextProtocol 0.7.0 |
| `aichatweb` | `Microsoft.Extensions.AI.Templates` | `10.3.0-preview.3.26109.11` | M.E.AI 10.3.0, DataIngestion, SqliteVec |
| `aiagent-webapi` | `Microsoft.Agents.AI.ProjectTemplates` | `1.0.0-preview.1.25619.3` | Microsoft.Agents.AI 1.0.0, Workflows, DevUI |
| `cea` | `M365Advocacy.M365Copilot.Templates` | `0.0.4` | M365 Agents SDK, Bot Framework |

## What Changed: v9.x to v10.x

| Area | v9.x | v10.x |
|------|------|-------|
| Packaging | 1 NuGet package | 3 separate packages |
| MCP SDK | ModelContextProtocol 0.3.x | 0.7.0 with registry support |
| AI abstractions | M.E.AI 9.x | 10.3.0 (ReasoningOptions, cached tokens) |
| Agent framework | None | Microsoft.Agents.AI 1.0.0 with workflows |
| RAG | Qdrant vector store | DataIngestion + SQLite vector store |
| Agent testing | None | DevUI at `/devui` |
| OTel conventions | v1.35 | v1.38 |

## Scoring Cheat Sheet

### Track 1 — Creative Apps
| Criterion | Weight |
|-----------|--------|
| Accuracy & Relevance | 20% |
| Reasoning & Multi-step Thinking | 20% |
| Creativity & Originality | 15% |
| User Experience & Presentation | 15% |
| Reliability & Safety | 20% |
| Community Vote | 10% |

### Track 2 — Reasoning Agents
| Criterion | Weight |
|-----------|--------|
| Accuracy & Relevance | 25% |
| Reasoning & Multi-step Thinking | 25% |
| Creativity & Originality | 15% |
| User Experience & Presentation | 15% |
| Reliability & Safety | 20% |

### Track 3 — Enterprise Agents
| Criterion | Points | Status |
|-----------|--------|--------|
| M365 Copilot Chat Agent | Required | Must have |
| External MCP Server (Read/Write) | 8 pts | Optional |
| OAuth Security for MCP | 5 pts | Optional |
| Adaptive Cards UI/UX | 5 pts | Optional |
| Connected Agents Architecture | 15 pts | Highest bonus |

## Links

- [Agents League Discord](https://aka.ms/agentsleague/discord)
- [Copilot Dev Camp](https://aka.ms/copilotdevcamp)
- [Agent Academy](https://aka.ms/agentacademy)
- [Microsoft Foundry](https://ai.azure.com)
- [dotnet/extensions releases](https://github.com/dotnet/extensions/releases)

## License

Track READMEs contain Microsoft's official hackathon rules (MIT licensed via [microsoft/agentsleague](https://github.com/microsoft/agentsleague)).
Templates are scaffolded from official Microsoft .NET project templates.
Starter kit curation and documentation by [@AncpLua](https://github.com/AncpLua).
