// =============================================================================
// MAF + qyl Integration Sample
//
// Demonstrates how qyl replaces Application Insights, Aspire Dashboard, and
// Grafana as the single observability backend for Microsoft Agent Framework.
//
// Problem: App Insights truncates gen_ai.input.messages at 8KB.
// Solution: qyl stores full messages in DuckDB with no size limit.
//
// Usage:
//   1. Start qyl collector:  dotnet run --project src/qyl.collector
//   2. Run this sample:      dotnet run --project samples/maf-agent-qyl
//   3. Open dashboard:       http://localhost:5100
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qyl.Samples.MafAgent;
using Qyl.ServiceDefaults.Instrumentation;

var builder = Host.CreateApplicationBuilder(args);

// ONE LINE replaces App Insights + Aspire Dashboard + Grafana:
builder.UseQyl(o => o.EnableOpenApi = false);

builder.Services.AddHostedService<AgentDemo>();

var app = builder.Build();
await app.RunAsync();
