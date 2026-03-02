# Track 2 — CertPrepAgents Development Reference

## Architecture: 5-Agent Pipeline

```
Student prompt
    → Orchestrator
    → Curator Agent          — finds relevant Learn modules
    → Study Plan Generator   — produces structured study plan
    → Assessment Agent       — generates cert exam questions (o4-mini), grades answers
    → Certification Planner  — pass: next cert / fail: targeted remediation
         ↑___________________|
              (loop on fail — weak domains only)
```

## Inter-Agent Contracts

- `CuratorOutput` → Study Plan Generator
- `StudyPlan` → Assessment Agent
- `AssessmentResult` → Certification Planner
- `RemediationPlan` → loop back to Curator (targeted domains only)

## Key Technical Decisions

| Decision | Why |
|----------|-----|
| Reasoning model for assessment | o4-mini produces higher-quality exam questions and more accurate grading |
| Live Microsoft Learn API | Real content retrieval — not hallucinated URLs |
| Remediation loop (not restart) | Diagnoses which domains failed and why, targets only those |
| Content safety middleware | Runs before any agent sees input |

See `track-2-reasoning-agents/README.md` for full details.
