# Track 3 — Enterprise Support Agent Development Reference

## Architecture: DA + CEA + 3 Connected Agents

```
M365 Copilot Chat
    → DA (declarativeAgent.json)
    → CEA (Bot.cs — orchestrator with intent routing)
    → TicketAgent    — CRUD via MCP server
    → KnowledgeAgent — searches embedded knowledge base + MCP observability
    → NotifyAgent    — team notifications with approval workflows
```

## Connected Agent Routing

The orchestrator classifies user intent via LLM and routes to the appropriate agent. Each agent operates independently with its own system prompt and response format.

## MCP Integration

- **SupportMcpServer** — Standalone .NET MCP server (stdio transport) with CreateTicket, UpdateTicket, GetTicket
- **MCP Observability** — Connects to external MCP server for live telemetry queries

## Adaptive Cards

- ResultCard — structured output with status, facts, and actions
- ConfirmationCard — notification confirmation with approve/reject

See `track-3-enterprise-agents/README.md` for full details.
