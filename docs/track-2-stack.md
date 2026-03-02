# Track 2 Stack — Reasoning Agents

## SDKs Used

1. **Microsoft.Extensions.AI** — Agent orchestration and chat client abstraction
2. **Microsoft Learn MCP Server** — Real certification content retrieval
3. **OpenTelemetry** — Distributed tracing for agent handoffs

## 5-Agent Architecture

| Agent | Model | Responsibility |
|-------|-------|---------------|
| Curator | gpt-4.1-mini | Finds Microsoft Learn modules for certification domains |
| Study Plan Generator | gpt-4.1-mini | Structures modules into weighted study plan |
| Assessment Agent | o4-mini | Generates scenario-based questions, grades with reasoning |
| Certification Planner | gpt-4.1-mini | Pass/fail decision with targeted remediation |
| Engagement Agent | gpt-4.1-mini | Sends follow-up communications |

## Pass/Fail Loop

The Certification Planner analyzes per-domain scores. If any domain is below 70%, it produces a targeted remediation plan for only those domains — not a full restart.

See `track-2-reasoning-agents/README.md` for full details.
