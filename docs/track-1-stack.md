# Track 1 Stack — Creative Apps + GitHub Copilot

## SDKs Used

1. **GitHub Copilot CLI SDK** — Programmatic access to Copilot via stdio/TCP JSON-RPC
2. **ModelContextProtocol .NET SDK** — MCP server with stdio transport
3. **Microsoft.Extensions.AI** — Model-agnostic IChatClient abstraction

## MCP Server Pattern

```csharp
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CodeIntelligenceTools>();
```

Tools are C# methods decorated with `[McpServerTool]` and `[Description]`. The server publishes as a NuGet tool (`PackageType=McpServer`).

## Distribution

`dotnet tool install` → add to `.mcp/server.json` → works in any Copilot instance. Zero Azure spend via GitHub Models free tier.

See `track-1-creative-apps/README.md` for full details.
