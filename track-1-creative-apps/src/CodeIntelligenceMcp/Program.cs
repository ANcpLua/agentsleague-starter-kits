using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// IChatClient via GitHub Models free tier
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN environment variable");
var chatClient = new ChatClient(
    "gpt-4o-mini",
    new ApiKeyCredential(token),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
    .AsIChatClient();

builder.Services.AddSingleton<IChatClient>(chatClient);

// OTel self-instrumentation
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(McpActivitySource.SourceName)
        .AddOtlpExporter());

// MCP server with tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CodeIntelligenceTools>()
    .WithTools<OtelInsightTools>()
    .WithTools<ArchitectureTools>();

await builder.Build().RunAsync();
