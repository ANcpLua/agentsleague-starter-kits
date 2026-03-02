# Enterprise Support Agent

**Microsoft Agents League Contest — Track 3: Enterprise Agents**

An AI-powered IT helpdesk that runs inside **Microsoft 365 Copilot Chat**, built with the Microsoft Agents SDK and M365 Agents Toolkit. Three connected agents collaborate to handle support tickets, search enterprise knowledge, and send team notifications — all through rich Adaptive Cards.

---

## What It Does

Enterprise Support Agent solves a universal enterprise problem: IT support is slow, fragmented, and frustrating. Employees bounce between ticketing portals, wikis, and Slack channels trying to get help. This agent brings everything into one conversational interface inside M365 Copilot Chat.

**Key capabilities:**

- **Ticket Management** — Create, update, escalate, and track IT support tickets through natural language
- **Knowledge Base Search** — Instant answers from 25 enterprise knowledge documents (150+ KB) covering HR, IT devices, project management, operations, and more
- **Team Notifications** — Send alerts and escalation notices to IT teams with approval workflows
- **Live Observability** — Query real-time telemetry data via MCP integration (traces, spans, metrics)
- **User Context** — Automatically fetches the signed-in user's M365 profile (name, department, role) via Microsoft Graph

---

## Architecture

```
                    Microsoft 365 Copilot Chat
                              |
                    +---------+---------+
                    |   Orchestrator    |
                    |     (Bot.cs)      |
                    |  Intent Routing   |
                    +---------+---------+
                   /          |          \
          +-------+    +------+------+    +-------+
          |Ticket |    | Knowledge  |    | Notify |
          | Agent |    |   Agent    |    | Agent  |
          +---+---+    +-----+------+    +---+----+
              |              |               |
         Adaptive       Embedded KB     Confirmation
          Cards        + MCP Tools        Cards
              |              |               |
        ResultCard     Knowledge/       ConfirmationCard
         (status,       *.md files      (Approve/Reject)
         actions)          |
                    MCP Server
                   (qyl.info)
```

### Connected Agents (15 bonus points)

| Agent | Responsibility | Output |
|-------|---------------|--------|
| **TicketAgent** | Ticket operations via AI extraction (create, update, get, list) | ResultCard with status, priority, actions |
| **KnowledgeAgent** | Searches embedded enterprise knowledge base + live observability via MCP | ResultCard with citations and sources |
| **NotifyAgent** | Prepares and sends team notifications with approval flow | ConfirmationCard with approve/reject actions |

The orchestrator (`Bot.cs`) classifies user intent via LLM and routes to the appropriate connected agent. Each agent operates independently with its own system prompt and response format.

### External MCP Server (8 bonus points)

Two MCP integrations:

1. **SupportMcpServer** — A standalone .NET MCP server (`stdio` transport) providing ticket CRUD tools:
   - `CreateTicket` — Creates a new support ticket (write operation)
   - `UpdateTicket` — Updates status, priority, or assignee (write operation)
   - `GetTicket` — Retrieves ticket details (read operation)

2. **MCP Observability** — Connects to an external MCP server (`mcp.qyl.info/sse`) for live telemetry queries:
   - Queries traces, spans, metrics, and log records
   - LLM-driven tool selection from available MCP tools
   - Results presented as Adaptive Cards with source attribution

### Adaptive Cards (5 bonus points)

Adaptive Card templates for rich interactive UI:

| Card | Purpose | Status |
|------|---------|--------|
| **ResultCard** | Structured output with title, status badge, facts, and action buttons | Active — used by TicketAgent and KnowledgeAgent |
| **ConfirmationCard** | Notification confirmation with approve/reject/request-info | Active — used by NotifyAgent |
| **TicketCard** | Ticket details with escalate/close/view actions | Template ready |
| **EscalationCard** | Escalation approval with approve/reject workflow | Template ready |
| **InputCard** | Form collection with text fields, dropdowns, and toggles | Template ready |

### M365 Copilot Chat Agent (Required)

Deployed as a Custom Engine Agent (CEA) via M365 Agents Toolkit:
- Declarative Agent manifest with custom instructions
- Entra ID authentication with Microsoft Graph SSO
- Bot Framework integration with streaming responses
- Copilot Chat scopes: `copilot`, `personal`, `team`, `groupChat`

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 10.0 (LTS) |
| Agent Framework | Microsoft Agents SDK v1.4 |
| AI Provider | GitHub Models (gpt-4.1-mini) / Azure OpenAI |
| MCP | ModelContextProtocol v0.7.0-preview.1 (C# SDK) |
| Authentication | Microsoft Entra ID + MSAL |
| State Storage | Azure Blob Storage (Azurite for local dev) |
| Observability | OpenTelemetry + Azure Monitor |
| Deployment | M365 Agents Toolkit (ATK) |
| Knowledge Base | 25 embedded markdown resources (150+ KB across 9 domains) |

---

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with M365 Agents Toolkit
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- Microsoft 365 tenant with sideloading enabled
- [GitHub Models](https://github.com/marketplace/models) API key (free tier)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/ancplua/agentsleague-starter-kits.git
   cd agentsleague-starter-kits/track-3-enterprise-agents
   ```

2. **Configure environment variables**
   ```bash
   cp src/EnterpriseSupportAgent/.env.example src/EnterpriseSupportAgent/.env
   # Edit .env with your values (see .env.example for all options)
   ```

3. **Build**
   ```bash
   dotnet build src/EnterpriseSupportAgent/EnterpriseSupportAgent.csproj
   dotnet build src/SupportMcpServer/SupportMcpServer.csproj
   ```

4. **Run locally with M365 Agents Toolkit**
   - Open the project in Visual Studio
   - Press F5 to provision Azure resources and launch in Teams/Copilot
   - The ATK will create Entra app registrations, Azure Bot, and storage automatically

5. **Test the MCP Server standalone**
   ```bash
   dotnet run --project src/SupportMcpServer/SupportMcpServer.csproj
   ```

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `LANGUAGE_MODEL_NAME` | AI model identifier (e.g., `openai/gpt-4.1-mini`) | Yes |
| `LANGUAGE_MODEL_ENDPOINT` | AI provider endpoint | Yes |
| `LANGUAGE_MODEL_KEY` | API key for the AI provider | Yes |
| `BOT_ID` | Azure Bot app registration ID | Yes (auto-provisioned by ATK) |
| `McpObservability__Endpoint` | MCP observability server URL | No (defaults to `mcp.qyl.info/sse`) |

---

## Project Structure

```
track-3-enterprise-agents/
  src/
    EnterpriseSupportAgent/          # Custom Engine Agent (CEA)
      Agents/
        IConnectedAgent.cs           # Agent interface + response record
        TicketAgent.cs               # Ticket operations with AI extraction
        KnowledgeAgent.cs            # KB search + MCP observability
        NotifyAgent.cs               # Notification approval flows
      AdaptiveCards/
        AdaptiveCardHelper.cs        # Template binding engine
        ResultCard.json              # Structured output card
        TicketCard.json              # Ticket details + actions
        EscalationCard.json          # Escalation approval
        ConfirmationCard.json        # Notification confirmation
        InputCard.json               # Form collection
      Knowledge/                     # Embedded enterprise KB (25 docs, 9 domains)
        hr/, it-devices/, it-solution/, manager/, ...
      Services/
        IChatClientService.cs        # AI provider abstraction
        OpenAiChatClientService.cs   # GitHub Models / OpenAI
        AzureOpenAiChatClientService.cs  # Azure OpenAI
        ChatClientFactory.cs         # Provider factory
        McpObservabilityService.cs   # MCP client for live telemetry
      Models/
        TicketInfo.cs, KnowledgeResult.cs, AgentJsonContext.cs
      M365Agent/                     # ATK project (manifest, infra, env)
        appPackage/manifest.json     # Teams/Copilot manifest
        appPackage/declarativeAgent.json  # Declarative agent config
      Bot.cs                         # Orchestrator — intent classification + routing
      Program.cs                     # Host setup, DI, middleware
    SupportMcpServer/                # Standalone MCP Server
      Tools/SupportTicketTools.cs    # CreateTicket, UpdateTicket, GetTicket
      Models/                        # Ticket, ITicketStore, InMemoryTicketStore
      Program.cs                     # MCP host with stdio transport
```

---

## Responsible AI

This agent implements several responsible AI practices:

- **Content Safety** — Input guardrail architecture designed for Azure Content Safety integration
- **Least Privilege** — Microsoft Entra ID authentication with scoped permissions; users only access their own tickets and authorized knowledge
- **Transparency** — The agent clearly identifies itself as AI-powered; Adaptive Cards show data sources and tool attributions
- **Human-in-the-Loop** — Notification and escalation flows require explicit user confirmation before executing irreversible actions
- **Privacy** — No PII stored beyond the current session; conversation state uses Azure Blob Storage with managed identity access
- **No Hallucination Guardrail** — KnowledgeAgent answers only from provided context; explicitly states "I don't have information about that" when the answer isn't in the knowledge base
- **Audit Trail** — OpenTelemetry instrumentation captures agent interactions for monitoring without exposing sensitive data

---

## License

[MIT License](./LICENSE)
