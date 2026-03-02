# MAF + qyl: Full Agent Observability Without Truncation

Demonstrates qyl replacing Application Insights + Aspire Dashboard + Grafana as the single observability backend for Microsoft Agent Framework agents.

## The Problem

- **[#4323](https://github.com/microsoft/agent-framework/discussions/4323)**: App Insights truncates `gen_ai.input.messages` at 8KB. Long system prompts = user message gets cut off. Debugging impossible.
- **[#4336](https://github.com/microsoft/agent-framework/discussions/4336)**: MAF's DevUI is local-only — no auth, no scale, no production use.

## The Fix

```csharp
// ONE LINE — replaces App Insights + Aspire Dashboard + Grafana:
builder.UseQyl();
```

qyl stores full messages in DuckDB VARCHAR with no size limit. The dashboard runs at `:5100` with SSE streaming, GenAI-specific views, and token cost tracking.

## Run It

```bash
# Terminal 1: start qyl collector
dotnet run --project src/qyl.collector

# Terminal 2: run this sample
dotnet run --project samples/maf-agent-qyl

# Open http://localhost:5100 — full conversation visible in trace detail
```

## Verify Full Messages in DuckDB

```sql
SELECT length(json_extract(attributes_json, '$.gen_ai.input.messages'))
FROM spans
WHERE gen_ai_provider_name IS NOT NULL;
```
