# Unified Seer Framework Specification

Version: 2.0
Date: 2026-03-04
Owner: Engineering Lead / PM

This is the single implementation-ready framework spec for Seer-inspired capabilities in this repository.
It is intentionally written as executable requirements (no ambiguity labels, no deferred interpretation).

## 1. Objective

Build a Seer-inspired enterprise agent framework that is:
1. deterministic in runtime behavior,
2. secure by default,
3. observable in production,
4. implementable by any engineer using this document only.

## 2. System Architecture

The framework has seven concrete components:
1. Orchestrator host (`EnterpriseSupportAgent`) for routing and lifecycle.
2. Connected agents (`TicketAgent`, `KnowledgeAgent`, `NotifyAgent`).
3. MCP integration service for external observability tools.
4. MCP ticket server exposing CRUD tools.
5. Adaptive card rendering layer for structured responses.
6. Auth/token validation middleware.
7. State + telemetry infrastructure.

Primary implementation anchors:
1. [Program.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Program.cs)
2. [Bot.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs)
3. [TicketAgent.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/TicketAgent.cs)
4. [KnowledgeAgent.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs)
5. [NotifyAgent.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/NotifyAgent.cs)
6. [McpObservabilityService.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Services/McpObservabilityService.cs)
7. [AdaptiveCardHelper.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/AdaptiveCards/AdaptiveCardHelper.cs)
8. [SupportMcpServer Program.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/SupportMcpServer/Program.cs)
9. [SupportTicketTools.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/SupportMcpServer/Tools/SupportTicketTools.cs)
10. [AspNetExtensions.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/AspNetExtensions.cs)
11. [ConversationStateExtensions.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/ConversationStateExtensions.cs)

## 3. Functional Requirements

### 3.1 Orchestrator

`FR-001` The host SHALL process inbound traffic on `POST /api/messages` via the agent adapter.
- Anchor: [Program.cs:85](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Program.cs:85)

`FR-002` The orchestrator SHALL classify every message into exactly one intent: `ticket`, `knowledge`, or `notify`.
- Anchor: [Bot.cs:147](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs:147)

`FR-003` Invalid or empty classifier outputs SHALL route to `KnowledgeAgent`.
- Anchor: [Bot.cs:63](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs:63)

`FR-004` The orchestrator SHALL emit streaming progress updates before final response payload.
- Anchor: [Bot.cs:46](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs:46)

### 3.2 Ticket Agent and MCP Ticket Tools

`FR-010` `TicketAgent` SHALL classify sub-intent into `create`, `update`, `get`, `list`.
- Anchor: [TicketAgent.cs:38](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/TicketAgent.cs:38)

`FR-011` Create flow SHALL extract `TITLE`, `DESCRIPTION`, `PRIORITY` with deterministic fallbacks.
- Anchor: [TicketAgent.cs:63](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/TicketAgent.cs:63)

`FR-012` Update flow SHALL extract `TICKET_ID`, `STATUS`, `PRIORITY`, `ASSIGNEE`.
- Anchor: [TicketAgent.cs:112](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/TicketAgent.cs:112)

`FR-013` Ticket responses SHALL return adaptive cards with operational facts and action context.
- Anchor: [TicketAgent.cs:94](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/TicketAgent.cs:94)

`FR-014` The MCP ticket server SHALL expose stdio transport with registered ticket tools.
- Anchor: [SupportMcpServer Program.cs:15](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/SupportMcpServer/Program.cs:15)

`FR-015` Ticket MCP contract SHALL include `CreateTicket`, `UpdateTicket`, `GetTicket` with stable argument names.
- Anchor: [SupportTicketTools.cs:9](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/SupportMcpServer/Tools/SupportTicketTools.cs:9)

`FR-016` Ticket storage access SHALL go through `ITicketStore` abstraction.
- Anchor: [ITicketStore.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/SupportMcpServer/Models/ITicketStore.cs)

### 3.3 Knowledge and Observability Agent

`FR-020` Knowledge questions SHALL be answered from embedded markdown corpus with source attribution.
- Anchor: [KnowledgeAgent.cs:173](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs:173)

`FR-021` Observability prompts SHALL be detected by term matching and routed through MCP tools.
- Anchor: [KnowledgeAgent.cs:57](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs:57)

`FR-022` MCP observability execution SHALL follow fixed sequence:
1. list tools,
2. choose tool + args,
3. execute tool,
4. synthesize answer,
5. return attributed card.

- Anchors: [KnowledgeAgent.cs:65](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs:65), [KnowledgeAgent.cs:95](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs:95), [KnowledgeAgent.cs:118](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs:118)

`FR-023` MCP failures SHALL trigger graceful fallback to embedded knowledge mode.
- Anchor: [KnowledgeAgent.cs:49](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs:49)

### 3.4 Notification Agent

`FR-030` `NotifyAgent` SHALL extract `TITLE`, `DESCRIPTION`, `RECIPIENTS`, `URGENCY`.
- Anchor: [NotifyAgent.cs:18](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/NotifyAgent.cs:18)

`FR-031` Notification actions SHALL require explicit user confirmation via confirmation card flow.
- Anchor: [NotifyAgent.cs:47](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/NotifyAgent.cs:47)

`FR-032` Invoke actions SHALL return deterministic action-specific acknowledgements.
- Anchor: [Bot.cs:95](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs:95)

### 3.5 Cards, Auth, and State

`FR-040` Result cards SHALL contain title, subtitle, status, statusColor, summary, and facts.
- Anchor: [AdaptiveCardHelper.cs:18](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/AdaptiveCards/AdaptiveCardHelper.cs:18)

`FR-041` Confirmation cards SHALL contain request identity and action payload hooks.
- Anchor: [AdaptiveCardHelper.cs:80](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/AdaptiveCards/AdaptiveCardHelper.cs:80)

`FR-042` Template binding SHALL support scalar and indexed array placeholders.
- Anchor: [AdaptiveCardHelper.cs:120](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/AdaptiveCards/AdaptiveCardHelper.cs:120)

`FR-050` JWT token validation SHALL enforce audience and issuer rules from configuration.
- Anchor: [AspNetExtensions.cs:54](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/AspNetExtensions.cs:54)

`FR-051` Conversation history SHALL be persisted and retrievable from conversation state.
- Anchor: [ConversationStateExtensions.cs:14](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/ConversationStateExtensions.cs:14)

`FR-052` User profile SHALL be cached in conversation state.
- Anchor: [ConversationStateExtensions.cs:48](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/ConversationStateExtensions.cs:48)

`FR-053` `-reset` SHALL clear auth/session state and conversation memory.
- Anchor: [Bot.cs:138](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs:138)

## 4. Seer-Equivalent Capability Layer

`SR-001` The framework SHALL generate LLM summaries for knowledge and telemetry outputs.
`SR-002` The framework SHALL support natural-language tool invocation over MCP.
`SR-003` The framework SHALL provide interactive investigation via connected agents.
`SR-004` The framework SHALL summarize trace/telemetry outputs into actionable insights.
`SR-005` The framework SHALL enforce secure calls and controlled failure disclosure.

Execution anchors are the union of `KnowledgeAgent`, `McpObservabilityService`, `Bot`, and `AspNetExtensions`.

## 5. Configuration Contract

Required configuration:
1. `BOT_ID`
2. `MICROSOFT_APP_ID`
3. `MICROSOFT_APP_PASSWORD`
4. `MICROSOFT_APP_TENANT_ID`
5. `LanguageModel:Name`
6. `LanguageModel:Endpoint`
7. `LanguageModel:ApiKey`

Optional configuration:
1. `McpObservability:Endpoint` (default `https://mcp.qyl.info/sse`)
2. `BlobsStorageOptions:StorageAccountName`
3. `BlobsStorageOptions:ContainerName`

Anchors:
- [.env.example](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/.env.example)
- [ChatClientFactory.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Services/ChatClientFactory.cs)
- [Program.cs](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Program.cs)

## 6. Non-Functional Requirements

`NFR-001` MCP and network failures SHALL degrade gracefully with warnings (not crashes).
- Anchor: [McpObservabilityService.cs:72](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Services/McpObservabilityService.cs:72)

`NFR-002` Production SHALL emit OpenTelemetry traces for host and tool operations.
- Anchors: [Program.cs:20](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Program.cs:20)

`NFR-003` Secrets SHALL never be committed; placeholder config SHALL remain in `.env.example`.
- Anchor: [.env.example:4](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/.env.example:4)

`NFR-004` Structured responses SHALL prefer adaptive cards; text fallback allowed only for failures.
- Anchor: [Bot.cs:75](/Users/ancplua/RiderProjects/agentsleague-starter-kits/track-3-enterprise-agents/src/EnterpriseSupportAgent/Bot.cs:75)

`NFR-005` If a web UI is introduced, styling SHALL use version-pinned design-token mapping with regression checks to avoid framework upgrade drift.
- Rule applies to future CSS/Tailwind adoption.

## 7. Acceptance Criteria

Release gate requires all conditions:
1. all `FR-*` requirements validated by integration tests,
2. all `SR-*` requirements validated by end-to-end scenarios,
3. auth rejects invalid issuer/audience tokens,
4. MCP outage path returns graceful user output,
5. adaptive cards render in target channels,
6. startup fails fast on missing required configuration,
7. no secrets detected in changed files.

## 8. Test Plan (Mandatory)

Required suites:
1. orchestrator intent routing and fallback tests,
2. ticket extraction and action tests,
3. knowledge retrieval + citation tests,
4. MCP tool selection/parse/call tests,
5. adaptive card template binding tests,
6. token validation tests,
7. conversation cache/reset tests.

Required scenarios:
1. create/update/get/list ticket end-to-end,
2. observability query end-to-end via MCP,
3. notification confirmation flow,
4. `-reset` state clearing verification.

## 9. Delivery Milestones

`M1` Runtime hardening
1. startup config validation,
2. feature toggles for connected-agent routes,
3. health status endpoints.

`M2` Data reliability
1. replace in-memory ticket storage with persistent backend,
2. add list ticket MCP tool,
3. add audit records for mutable operations.

`M3` Advanced Seer-aligned workflows
1. explicit RCA session model,
2. typed trace-summary contract,
3. anomaly insight contract over telemetry tools,
4. provider extension model for additional LLM backends.

## 10. Definition of Done

A release is complete only when:
1. Sections 3-9 requirements are satisfied,
2. tests and scenarios pass,
3. implementation anchors remain valid,
4. operational docs are updated for new config, flags, and failure modes.
