# Seer Framework Specification

| Field | Value |
|---|---|
| Status | Final |
| Version | 2.0 |
| Last Updated | 2026-03-04 |
| Owners | Platform Engineering + AI Engineering |
| Purpose | Single source of truth for building and operating Seer |

This document is the implementation contract for Seer. Teams SHALL implement to this specification.

## 1. Product Objectives

Seer SHALL provide the following outcomes:

1. Reduce issue noise by semantic grouping and deduplication.
2. Generate actionable issue and trace summaries.
3. Score fixability and route automation accordingly.
4. Run root-cause analysis and produce fix plans.
5. Generate code changes and optionally open pull requests.
6. Perform AI code review for pull requests.
7. Provide interactive investigation via Explorer.
8. Detect anomalies in time-series data.
9. Offer assisted natural-language query and search workflows.
10. Expose all capabilities through stable APIs and operational controls.

## 2. Engineering Principles

1. Contract-first: every capability SHALL have typed request and response schemas.
2. Deterministic boundaries: all service-to-service calls SHALL be authenticated and signed.
3. Safe automation: automation SHALL be gated by explicit thresholds, quotas, and kill switches.
4. Explainability: each automated decision SHALL include machine-readable reason codes.
5. Incremental rollout: every high-impact feature SHALL support org and project rollout controls.
6. Auditability: every run SHALL emit lifecycle events and trace identifiers.
7. Operability: each service SHALL expose health checks, metrics, and timeout budgets.

## 3. Scope Taxonomy

Seer capabilities are organized into five implementation domains.

| Domain | Capabilities |
|---|---|
| Intelligence Core | Grouping, Summarization, Fixability Scoring, Root Cause, Anomaly Detection |
| Automation | Autofix Pipeline, PR creation, lifecycle orchestration |
| Developer Workflow | AI Code Review, Test Generation, Coding Agent handoff |
| Investigation UX | Explorer, Assisted Query, Trace Summarization |
| Platform | APIs, RPC bridge, data model, configuration, security, operations |

## 4. System Architecture

Seer SHALL run as a multi-service system with a Sentry-facing control plane.

### 4.1 Logical Components

| Component | Responsibility |
|---|---|
| Sentry Control Plane (Django) | Public APIs, auth, orchestration, webhooks, task dispatch |
| Autofix/Explorer Service | Root cause, solution planning, code generation orchestration |
| Summarization Service | Issue and trace summary generation |
| Grouping Service | Embedding generation and nearest-neighbor matching |
| Scoring Service (GPU) | Fixability scoring |
| Anomaly Detection Service | Time-series anomaly analysis and storage |
| Code Review Service | PR review analysis and GitHub status/comment publishing |
| External Integrations | GitHub, Slack, MCP clients |

### 4.2 Runtime and Infrastructure

| Area | Required Implementation |
|---|---|
| Service runtime | Python 3.11 |
| API transport | HTTP/JSON for Sentry-to-Seer and Seer-to-Sentry RPC |
| Worker runtime | Celery workers for asynchronous execution |
| Queue | RabbitMQ |
| Primary data store | PostgreSQL |
| Vector search | pgvector with HNSW index |
| Model artifact storage | GCS bucket `gs://sentry-ml/seer/models` |
| AI operation tracing | Langfuse |

### 4.3 Signed Communication Contract

All Sentry-to-Seer requests SHALL use HMAC-SHA256 signing.

| Item | Requirement |
|---|---|
| Secret | `SEER_API_SHARED_SECRET` |
| Auth header | `Authorization: Rpcsignature rpc0:{signature}` |
| Optional viewer context | `X-Viewer-Context`, `X-Viewer-Context-Signature` |
| Client implementation | pooled HTTP client (urllib3 or equivalent) |

All Seer-to-Sentry RPC callbacks SHALL target:

- `POST /api/0/internal/seer-rpc/`
- Authenticated with `SEER_RPC_SHARED_SECRET`

## 5. Service Topology

Seer SHALL expose the following service URLs and responsibilities.

| Service | Setting | Connection Pool | Responsibilities |
|---|---|---|---|
| Autofix/Explorer | `SEER_AUTOFIX_URL` | `seer_autofix_default_connection_pool` | Autofix, Explorer, coding-agent workflows, project preferences, model listing, assisted query |
| Summarization | `SEER_SUMMARIZATION_URL` | `seer_summarization_default_connection_pool` | Issue and trace summaries |
| Anomaly Detection | `SEER_ANOMALY_DETECTION_URL` | `seer_anomaly_detection_default_connection_pool` | Time-series anomaly ingestion and analysis |
| Grouping | `SEER_GROUPING_URL` | `seer_grouping_connection_pool` | Similarity grouping |
| Breakpoint Detection | `SEER_BREAKPOINT_DETECTION_URL` | `seer_breakpoint_connection_pool` | Performance breakpoint detection |
| Scoring (GPU) | `SEER_SCORING_URL` | `fixability_connection_pool_gpu` | Fixability scoring |
| Code Review | `SEER_PREVENT_AI_URL` | `seer_code_review_connection_pool` | AI PR review and reruns |

## 6. Capability Specifications

### 6.1 Issue Grouping and Similarity

Seer SHALL group new events using semantic similarity when hash grouping does not match.

| Property | Required Value |
|---|---|
| Embedding model | `jinaai/jina-embeddings-v2-base-en` |
| Vector dimension | 768 |
| Search index | HNSW in pgvector |
| Query strategy | two-stage (permissive candidate search, strict final match filter) |
| Model versioning | `SEER_SIMILARITY_MODEL_VERSION` + grouping version enum |

`GroupHashMetadata` SHALL persist:

- `seer_date_sent`
- `seer_event_sent`
- `seer_model`
- `seer_matched_grouphash`
- `seer_match_distance`
- `seer_latest_training_model`

Acceptance criteria:

1. New unmatched events are processed by grouping pipeline within SLA.
2. Matched group hash and distance are stored for audit.
3. Similarity decisions are reproducible from stored model version and input payload.

### 6.2 Issue Summarization

Seer SHALL generate structured issue summaries.

Required response schema fields:

- `headline`
- `whats_wrong`
- `trace`
- `possible_cause`
- `scores`

Caching requirements:

| Key | TTL |
|---|---|
| `ai-group-summary-v2:{group_id}` | 7 days |

Generation SHALL use distributed locking by group id.

Acceptance criteria:

1. Repeated requests within TTL return cache-hit responses.
2. Summary payload is schema-valid.
3. Concurrent generation requests produce one canonical result.

### 6.3 Fixability Scoring

Seer SHALL score issues on `[0.0, 1.0]` and classify into automation bands.

| Range | Label | Default automation behavior |
|---|---|---|
| `< 0.25` | `SUPER_LOW` | No automation |
| `0.25–0.40` | `LOW` | Controlled by automation tuning |
| `0.40–0.66` | `MEDIUM` | Root-cause only |
| `0.66–0.76` | `HIGH` | Generate code changes |
| `>= 0.78` | `SUPER_HIGH` | Open PR eligible |

Acceptance criteria:

1. Scores are persisted with run metadata.
2. Classification is deterministic for a given score.
3. Automation routing reads classification, not ad-hoc thresholds.

### 6.4 Root Cause Analysis

Root-cause analysis SHALL be stage 1 of Autofix and Explorer investigation.

Inputs SHALL include:

- Error and issue metadata
- Stack traces
- Trace spans
- Structured logs
- Relevant source context

Outputs SHALL include:

- Primary cause statement
- Supporting evidence list
- Confidence score
- Next-step recommendation

Lifecycle events SHALL include:

- `seer.root_cause_started`
- `seer.root_cause_completed`

### 6.5 Autofix Pipeline

Autofix SHALL execute this ordered state machine:

1. Root cause analysis
2. Solution planning
3. Code change generation
4. Pull request creation (optional)

Automation tuning SHALL support:

- `off`
- `super_low`
- `low`
- `medium`
- `high`
- `always`

Stopping points SHALL support:

- `code_changes`
- `open_pr`

Run lifecycle events SHALL include:

- `seer.root_cause_started`
- `seer.root_cause_completed`
- `seer.solution_started`
- `seer.solution_completed`
- `seer.coding_started`
- `seer.coding_completed`
- `seer.pr_created`

### 6.6 AI Code Review (Seer Prevent)

AI code review SHALL run from GitHub webhook triggers:

- pull request opened (non-draft)
- draft to ready-for-review
- new commit on ready PR
- comment trigger (`@sentry review`)
- check-run rerequest

Review output SHALL include:

- inline PR comments
- GitHub check status

Supported status states:

- success
- neutral
- error
- cancelled

Retention:

- review run records SHALL be retained for 90 days minimum.

### 6.7 Explorer

Explorer SHALL provide interactive investigation runs.

Run model SHALL include:

- `run_id`
- `organization_id`
- `project_id`
- `issue_id` (optional)
- `state`
- `timeline_events[]`
- `created_at`
- `updated_at`

Explorer SHALL support:

- conversational debugging
- trace and issue context retrieval
- autofix trigger from run context
- run list and run update APIs

### 6.8 Anomaly Detection

Anomaly detection SHALL store and evaluate time-series signals.

Service endpoint SHALL include:

- `POST /v1/anomaly-detection/store`

Capabilities SHALL include:

- baseline computation
- anomaly marking
- historical anomaly retrieval
- cohort comparison workflow

### 6.9 Trace Summarization

Trace summarization SHALL produce a typed summary from trace payloads.

Caching requirements:

| Key | TTL |
|---|---|
| `ai-trace-summary:{trace_slug}` | 7 days |

### 6.10 Assisted Query and Search Agent

Seer SHALL support natural language to query translation and stateful search sessions.

Required operations:

- translate query intent
- start search session
- retrieve session state
- create/reuse query cache entries

### 6.11 Test Generation

Seer SHALL generate unit test suggestions from issue and code context.

Primary endpoint:

- `POST /v1/automation/codegen/unit-tests`

Output contract SHALL include:

- target file mapping
- generated test cases
- rationale for each test case

## 7. API Surface

### 7.1 Public REST API (Control Plane)

| Endpoint | Method | Purpose |
|---|---|---|
| `/api/0/seer/models/` | GET | List active LLM model names |
| `/api/0/issues/{issue_id}/autofix/` | POST | Start autofix run |
| `/api/0/issues/{issue_id}/autofix/` | GET | Get autofix state |
| `/api/0/issues/{issue_id}/autofix/setup/` | GET | Check autofix prerequisites |
| `/api/0/issues/{issue_id}/autofix/update/` | POST | Update running autofix |
| `/api/0/issues/{issue_id}/ai-summary/` | POST | Generate AI issue summary |
| `/api/0/organizations/{org}/seer/setup-check/` | GET | Check quota and billing readiness |
| `/api/0/organizations/{org}/seer/onboarding-check/` | GET | Check onboarding status |
| `/api/0/organizations/{org}/autofix-automation-settings/` | GET/PUT | Read or update org automation settings |
| `/api/0/organizations/{org}/trace-summary/` | POST | Generate trace summary |
| `/api/0/organizations/{org}/seer-explorer/chat/` | POST | Send Explorer chat input |
| `/api/0/organizations/{org}/seer-explorer/runs/` | GET | List Explorer runs |
| `/api/0/organizations/{org}/seer-explorer/update/` | POST | Update Explorer run state |
| `/api/0/projects/{org}/{project}/seer-preferences/` | GET/PUT | Read or update project Seer preferences |
| `/api/0/organizations/{org}/search-agent/start/` | POST | Start assisted search |
| `/api/0/organizations/{org}/search-agent/state/` | GET | Read assisted search state |

### 7.2 Seer Service Endpoints (Sentry to Seer)

| Path | Service |
|---|---|
| `/v1/automation/autofix/start` | Autofix |
| `/v1/automation/autofix/update` | Autofix |
| `/v1/automation/autofix/state` | Autofix |
| `/v1/automation/autofix/state/pr` | Autofix |
| `/v1/automation/autofix/prompt` | Autofix |
| `/v1/automation/autofix/coding-agent/state/update` | Autofix |
| `/v1/automation/autofix/coding-agent/state/set` | Autofix |
| `/v1/automation/summarize/trace` | Summarization |
| `/v1/automation/summarize/issue` | Summarization |
| `/v1/automation/summarize/fixability` | Scoring |
| `/v1/automation/explorer/index` | Autofix |
| `/v1/automation/explorer/index/org-project-knowledge` | Autofix |
| `/v1/automation/codegen/unit-tests` | Autofix |
| `/v1/automation/codegen/pr-review/rerun` | Code Review |
| `/v1/automation/overwatch-request` | Code Review |
| `/v1/project-preference` | Autofix |
| `/v1/project-preference/set` | Autofix |
| `/v1/project-preference/bulk` | Autofix |
| `/v1/project-preference/bulk-set` | Autofix |
| `/v1/project-preference/remove-repository` | Autofix |
| `/v1/models` | Autofix |
| `/v1/llm/generate` | Autofix |
| `/v1/assisted-query/translate` | Autofix |
| `/v1/assisted-query/start` | Autofix |
| `/v1/assisted-query/state` | Autofix |
| `/v1/assisted-query/translate-agentic` | Autofix |
| `/v1/assisted-query/create-cache` | Autofix |
| `/v1/explorer/service-map/update` | Autofix |
| `/v0/issues/supergroups` | Autofix |
| `/v1/workflows/compare/cohort` | Anomaly Detection |

### 7.3 RPC Bridge (Seer to Sentry)

Endpoint:

- `POST /api/0/internal/seer-rpc/`

RPC categories SHALL include:

1. issue, event, span, and trace retrieval
2. organization and project metadata lookup
3. repository and code mapping lookup
4. feature flag checks
5. webhook broadcasting

## 8. Data Model and Persistence

### 8.1 Relational Models

`SeerOrganizationSettings` SHALL include:

- `organization` (FK, unique, indexed)
- `default_coding_agent` (enum/string)
- `default_coding_agent_integration_id` (integration FK)

`CodeReviewRun` SHALL track full review lifecycle states.

`CodeReviewEvent` SHALL store review event-level telemetry.

### 8.2 Migrations

Required migration baseline:

1. `0001_add_seerorganizationsettings.py`
2. `0002_add_default_coding_agent.py`

### 8.3 Cache Keys

| Key Pattern | TTL | Purpose |
|---|---|---|
| `ai-group-summary-v2:{group_id}` | 7 days | issue summary cache |
| `ai-trace-summary:{trace_slug}` | 7 days | trace summary cache |
| `seer-project-has-repos:{org_id}:{project_id}` | 15 minutes | repository presence cache |
| `seer:seat-based-tier:{org_id}` | 4 hours | billing tier cache |
| `autofix_access_check:{group_id}` | 1 minute | access gate cache |
| `SeerOperatorAutofixCache` | run lifetime | operator state cache |

## 9. Configuration and Controls

### 9.1 Required Settings

| Setting | Purpose |
|---|---|
| `SEER_AUTOFIX_URL` | Autofix and Explorer service base URL |
| `SEER_SUMMARIZATION_URL` | Summarization service base URL |
| `SEER_ANOMALY_DETECTION_URL` | Anomaly service base URL |
| `SEER_GROUPING_URL` | Grouping service base URL |
| `SEER_BREAKPOINT_DETECTION_URL` | Breakpoint service base URL |
| `SEER_SCORING_URL` | Scoring service base URL |
| `SEER_PREVENT_AI_URL` | Code review service base URL |
| `SEER_API_SHARED_SECRET` | Sentry to Seer signing key |
| `SEER_RPC_SHARED_SECRET` | Seer to Sentry signing key |
| `SEER_AUTOFIX_GITHUB_APP_USER_ID` | GitHub App user identity |
| `SEER_MAX_GROUPING_DISTANCE` | Similarity threshold |
| `SEER_ANOMALY_DETECTION_TIMEOUT` | anomaly request timeout |
| `SEER_BREAKPOINT_DETECTION_TIMEOUT` | breakpoint request timeout |
| `SEER_FIXABILITY_TIMEOUT` | scoring request timeout |
| `CLAUDE_CODE_CLIENT_CLASS` | coding agent adapter class |

### 9.2 Feature Flags

The platform SHALL provide org, project, and system level flags.

Required flags:

- `organizations:gen-ai-features`
- `organizations:seer-explorer`
- `organizations:autofix-on-explorer`
- `organizations:single-trace-summary`
- `organizations:code-review-experiments-enabled`
- `projects:similarity-grouping-v2-model`
- `projects:supergroup-embeddings-explorer`
- `seer-explorer`
- `triage-signals-v0-org`

### 9.3 Organization and Project Options

Organization options SHALL include:

- global AI kill switch (`sentry:hide_ai_features`)
- code generation toggle (`sentry:enable_seer_coding`)
- default automation tuning
- default PR behavior
- default coding agent

Project options SHALL include:

- scanner automation enablement
- automation tuning level

### 9.4 Rate Limits

Baseline limits SHALL be configurable through options:

- `seer.max_num_scanner_autotriggered_per_ten_seconds` default `15`
- `seer.max_num_autofix_autotriggered_per_hour` default `20`

Circuit breaker and retry configuration SHALL exist for similarity-grouping ingest.

## 10. Integration Requirements

### 10.1 GitHub

GitHub integration SHALL support:

1. repository content access
2. pull request comment and status publishing
3. webhook ingestion for pull request lifecycle events
4. PR tracking for open, close, merge transitions

### 10.2 Slack

Slack integration SHALL support:

1. lifecycle notifications
2. mention-triggered Explorer sessions
3. thread-context run continuation

### 10.3 MCP

MCP integration SHALL support:

1. triggering Seer analysis
2. retrieving fix recommendations
3. polling run status

## 11. Security and Data Handling

### 11.1 Security Controls

Seer SHALL enforce:

1. signed inter-service requests
2. strict authorization on public APIs
3. organization and project level kill switches
4. audit trails for run state transitions

### 11.2 Data Minimization Matrix

| Feature | Data Sent |
|---|---|
| AI Code Review | file names, diffs, PR description |
| Issue Fix and RCA | issue metadata, stack traces, traces, logs, relevant code context |
| Summarization | issue and trace context required by schema |

Seer SHALL NOT send full repository contents for code review unless explicitly required by a documented capability.

## 12. Operations and Reliability

### 12.1 Queue and Task Requirements

Core async tasks SHALL include:

- summary generation and automation dispatch
- autofix status reconciliation
- onboarding and setup checks
- update processing

### 12.2 SLO and Failure Handling

Required reliability behavior:

1. every endpoint SHALL define timeout budgets
2. failed async tasks SHALL use bounded retries
3. stale runs SHALL be detected and marked failed
4. lifecycle metrics SHALL be emitted for all run stages

### 12.3 Minimum Observability Metrics

The platform SHALL emit metrics for:

- request volume and latency by endpoint
- webhook accepted, filtered, errored, enqueued counts
- task enqueue and end-to-end latency
- cache hit/miss rates for summary endpoints
- automation trigger counts by tuning level

## 13. Implementation Plan (Executable Work Packages)

### WP1: Platform foundation

Deliverables:

1. service URL wiring and connection pools
2. HMAC signing implementation and RPC verification
3. core settings and secrets management

Exit criteria:

- signed request contract passes integration tests
- health checks are green for all services

### WP2: Data and cache layer

Deliverables:

1. baseline migrations
2. model definitions
3. cache-key contract implementation

Exit criteria:

- migration and rollback tested
- cache TTL policy validated

### WP3: Intelligence core

Deliverables:

1. grouping pipeline
2. summarization pipeline
3. fixability scoring pipeline
4. anomaly detection pipeline

Exit criteria:

- each capability passes schema contract tests
- each capability has documented SLO and alerting

### WP4: Autofix pipeline

Deliverables:

1. root-cause stage
2. solution stage
3. code generation stage
4. optional PR stage

Exit criteria:

- lifecycle events emitted in order
- stopping points respected

### WP5: Explorer and assisted query

Deliverables:

1. run model and APIs
2. chat/update/list flows
3. assisted-query start/state/translate flows

Exit criteria:

- run replay is deterministic from stored timeline
- API contract tests pass

### WP6: Code review and SCM integration

Deliverables:

1. webhook ingestion
2. review analysis execution
3. status and inline comment publishing

Exit criteria:

- trigger matrix passes end-to-end tests
- run retention and cleanup policy enforced

### WP7: Governance and rollout

Deliverables:

1. flag topology implementation
2. org and project options
3. rate limits and circuit breakers

Exit criteria:

- kill switch works globally
- tuning levels alter behavior as specified

### WP8: Verification and release hardening

Deliverables:

1. contract test suite
2. load and failure-injection tests
3. runbooks and on-call playbook

Exit criteria:

- definition of done criteria all met
- release sign-off approved

## 14. Verification Checklist

Before release, teams SHALL verify:

1. Public REST contracts: all routes implemented and schema-tested.
2. Internal Seer contracts: all `/v1` and `/v0` routes implemented and integration-tested.
3. RPC bridge: signed callbacks accepted, unsigned callbacks rejected.
4. Capability coverage: sections 6.1 to 6.11 each have green end-to-end tests.
5. Data guarantees: required fields and cache keys present with expected TTL.
6. Governance controls: flags, options, and quotas enforced.
7. Security controls: auth and audit trails validated.
8. Operability: dashboards, alerts, and runbooks complete.

## 15. Definition of Done

Seer implementation is complete only when:

1. All capabilities in this document are implemented.
2. All acceptance criteria and verification checklist items pass.
3. Production monitoring and incident response artifacts are in place.
4. The release is approved by Platform Engineering, AI Engineering, and Security.

## 16. Source References

Primary references for this specification:

- Sentry AI documentation
- Sentry API documentation
- `getsentry/sentry` Seer modules and endpoint contracts
- `getsentry/sentry-mcp` integration contracts
