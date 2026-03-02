# Track 1 — Code Intelligence MCP Server Development Reference

## Architecture: MCP Server with 6 Tools

```
Copilot Chat (VS Code or CLI)
    → discovers MCP server via .mcp/server.json
    → MCP server started (stdio transport)
    → tool call routed via JSON-RPC
    → tool method executes with IChatClient (GitHub Models free tier)
    → structured result returned to Copilot
    → Copilot reasons over the result and presents to user
```

## Tools

| Tool | What It Does |
|------|-------------|
| ExplainCode | Analyzes source code: algorithm identification, Big-O complexity, bug detection |
| ConvertCode | Converts code between languages with idiomatic patterns |
| SolveWithCode | Turns problem descriptions into parameterized, runnable code |
| AnalyzeVisual | Reviews UI design specs for accessibility and implementation guidance |
| GenerateArchitectureDiagram | Scans a codebase and generates Mermaid architecture diagrams |
| ExplainTrace | Transforms OTel JSON spans into human-readable narratives |

## Self-Instrumentation

Every tool call is instrumented with OpenTelemetry — the tool that explains code also explains itself.

See `track-1-creative-apps/README.md` for full details.
