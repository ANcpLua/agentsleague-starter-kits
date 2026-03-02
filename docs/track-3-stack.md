# Track 3 Stack — Enterprise Agents + M365

## SDKs Used

1. **Microsoft Agents SDK v1.4** — Bot Framework for M365 Copilot Chat
2. **ModelContextProtocol v0.7.0-preview.1** — C# MCP SDK
3. **M365 Agents Toolkit (ATK)** — Deployment pipeline with Entra ID and Bicep infra

## CEA Architecture

The Custom Engine Agent handles message routing, Adaptive Card rendering, and conversation state. Deployed via ATK to Azure App Service with Bot Framework integration.

## Authentication

Microsoft Entra ID + MSAL for SSO. Graph API integration for user profile context.

See `track-3-enterprise-agents/README.md` for full details.
