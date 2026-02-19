# File Map — Agents League Starter Kits

> Personal reference for ancplua. Every file, what it does, which track it belongs to.

## Root

| File | Purpose |
|------|---------|
| `README.md` | Master context — maps tracks to templates, install commands, scoring |
| `FILE-MAP.md` | THIS FILE — you are here |

## Track READMEs

| File | Track | What's inside |
|------|-------|---------------|
| `1-creative-apps/1-creative-apps-README.md` | Track 1 | Creative Apps rules, Copilot tips, .NET template options, eval criteria |
| `2-reasoning-agents/2-reasoning-agents-README.md` | Track 2 | Reasoning Agents rules, Foundry setup, multi-agent architecture, .NET alt path |
| `2-reasoning-agents/reasoning-agents-architecture.png` | Track 2 | Architecture diagram (student cert prep flow) |
| `3-enterprise-agents/README.md` | Track 3 | Enterprise Agents rules, DA/CEA/Copilot Studio guides, scoring matrix |

## Template: McpServerApp (MCP Server)

> `dotnet new mcpserver` | Package: `Microsoft.McpServer.ProjectTemplates 0.7.0-preview.1` | Best for: ALL tracks

| File | What it does |
|------|-------------|
| `McpServerApp/Program.cs` | Entry point — Host builder, stdio transport, tool registration |
| `McpServerApp/McpServerApp.csproj` | Project file — `PackAsTool`, `PackageType=McpServer`, self-contained, `PublishSingleFile` |
| `McpServerApp/Tools/RandomNumberTools.cs` | Example tool — `[McpServerTool]` attribute pattern, `[Description]` on params |
| `McpServerApp/.mcp/server.json` | MCP registry manifest — schema `2025-10-17`, NuGet package metadata |
| `McpServerApp/README.md` | Template's own README — how to run, test, publish |

## Template: AiChatWeb (AI Chat Web App)

> `dotnet new aichatweb` | Package: `Microsoft.Extensions.AI.Templates 10.3.0-preview` | Best for: Track 1, Track 2

| File | What it does |
|------|-------------|
| `AiChatWeb/Program.cs` | Entry point — GitHub Models client, SQLite vector store, RAG pipeline, Blazor SSR |
| `AiChatWeb/AiChatWeb.csproj` | Project file — M.E.AI 10.3.0, DataIngestion, PdfPig, SqliteVec, ML.Tokenizers |
| `AiChatWeb/appsettings.json` | Config — logging defaults |
| `AiChatWeb/appsettings.Development.json` | Dev config overrides |
| `AiChatWeb/Properties/launchSettings.json` | Launch profiles (ports, HTTPS) |
| `AiChatWeb/README.md` | Template's own README |
| **Services/** | |
| `AiChatWeb/Services/SemanticSearch.cs` | RAG retrieval — vector similarity search against SQLite |
| `AiChatWeb/Services/IngestedChunk.cs` | Data model for vector store chunks |
| `AiChatWeb/Services/Ingestion/DataIngestor.cs` | RAG ingestion pipeline — reads docs, chunks, embeds, stores |
| `AiChatWeb/Services/Ingestion/DocumentReader.cs` | Base document reader abstraction |
| `AiChatWeb/Services/Ingestion/PdfPigReader.cs` | PDF reader using PdfPig library |
| **Components/** | |
| `AiChatWeb/Components/App.razor` | Root Blazor component |
| `AiChatWeb/Components/Routes.razor` | Router setup |
| `AiChatWeb/Components/_Imports.razor` | Global using directives |
| `AiChatWeb/Components/Layout/MainLayout.razor` | Page layout shell |
| `AiChatWeb/Components/Layout/LoadingSpinner.razor` | Loading indicator |
| `AiChatWeb/Components/Layout/SurveyPrompt.razor` | Feedback prompt (can remove) |
| `AiChatWeb/Components/Pages/Chat/Chat.razor` | Main chat page |
| `AiChatWeb/Components/Pages/Chat/ChatInput.razor` | Message input box |
| `AiChatWeb/Components/Pages/Chat/ChatMessageItem.razor` | Single message bubble |
| `AiChatWeb/Components/Pages/Chat/ChatMessageList.razor` | Message list container |
| `AiChatWeb/Components/Pages/Chat/ChatHeader.razor` | Chat header bar |
| `AiChatWeb/Components/Pages/Chat/ChatCitation.razor` | RAG citation display |
| `AiChatWeb/Components/Pages/Chat/ChatSuggestions.razor` | Suggested prompts |
| `AiChatWeb/Components/Pages/Error.razor` | Error page |
| **wwwroot/** | |
| `AiChatWeb/wwwroot/app.css` | Global styles |
| `AiChatWeb/wwwroot/app.js` | Global JS |
| `AiChatWeb/wwwroot/favicon.ico` | Favicon |
| `AiChatWeb/wwwroot/Data/Example_Emergency_Survival_Kit.pdf` | Sample PDF for RAG demo |
| `AiChatWeb/wwwroot/Data/Example_GPS_Watch.md` | Sample markdown for RAG demo |
| `AiChatWeb/wwwroot/lib/dompurify/` | HTML sanitizer lib |
| `AiChatWeb/wwwroot/lib/marked/` | Markdown parser lib |
| `AiChatWeb/wwwroot/lib/markdown_viewer/` | Markdown rendering component |
| `AiChatWeb/wwwroot/lib/pdf_viewer/` | PDF rendering component |
| `AiChatWeb/wwwroot/lib/pdfjs-dist/` | PDF.js library |
| `AiChatWeb/wwwroot/lib/tailwindcss/` | Tailwind CSS (preflight only) |

## Template: AiAgentWebApi (AI Agent Web API)

> `dotnet new aiagent-webapi` | Package: `Microsoft.Agents.AI.ProjectTemplates 1.0.0-preview` | Best for: Track 2, Track 3

| File | What it does |
|------|-------------|
| `AiAgentWebApi/Program.cs` | Entry point — multi-agent workflow (writer+editor), DevUI, OpenAI Responses API |
| `AiAgentWebApi/AiAgentWebApi.csproj` | Project file — Microsoft.Agents.AI 1.0.0-preview, Workflows, DevUI |
| `AiAgentWebApi/appsettings.json` | Config — logging defaults |
| `AiAgentWebApi/Properties/launchSettings.json` | Launch profiles |
| `AiAgentWebApi/README.md` | Template's own README — GITHUB_TOKEN setup, DevUI usage |

## Template: CustomEngineAgent (M365 CEA)

> `dotnet new cea` | Package: `M365Advocacy.M365Copilot.Templates` | Best for: Track 3 only

| File | What it does |
|------|-------------|
| `CustomEngineAgent/Program.cs` | Entry point — Bot Framework, Azure Bot Service, chat client DI |
| `CustomEngineAgent/CustomEngineAgent.csproj` | Project file |
| `CustomEngineAgent/CustomEngineAgent.sln` | Solution file |
| `CustomEngineAgent/Bot.cs` | Bot message handler — turns, conversation state |
| `CustomEngineAgent/AspNetExtensions.cs` | ASP.NET integration helpers |
| `CustomEngineAgent/ConversationStateExtensions.cs` | Conversation state management |
| `CustomEngineAgent/UserProfile.cs` | User profile model |
| `CustomEngineAgent/GettingStarted.md` | Template getting started guide |
| `CustomEngineAgent/.gitignore` | Git ignore rules |
| `CustomEngineAgent/.gitattributes` | Git attributes |
| `CustomEngineAgent/scripts/post-create.ps1` | Post-scaffold PowerShell script (Windows only) |
| `CustomEngineAgent/Properties/launchSettings.json` | Launch profiles |
| `CustomEngineAgent/Properties/serviceDependencies.json` | VS service dependencies |
| `CustomEngineAgent/Properties/serviceDependencies.local.json` | Local service deps |
| **Services/** | |
| `CustomEngineAgent/Services/IChatClientService.cs` | Chat client interface |
| `CustomEngineAgent/Services/ChatClientFactory.cs` | Factory for creating chat clients |
| `CustomEngineAgent/Services/OpenAiChatClientService.cs` | OpenAI implementation |
| `CustomEngineAgent/Services/AzureOpenAiChatClientService.cs` | Azure OpenAI implementation |
| **M365Agent/** | |
| `CustomEngineAgent/M365Agent/M365Agent.atkproj` | ATK project file |
| `CustomEngineAgent/M365Agent/launchSettings.json` | ATK launch settings |
| `CustomEngineAgent/M365Agent/m365agents.yml` | Agent manifest (cloud) |
| `CustomEngineAgent/M365Agent/m365agents.local.yml` | Agent manifest (local) |
| `CustomEngineAgent/M365Agent/.gitignore` | ATK git ignore |
| `CustomEngineAgent/M365Agent/appPackage/manifest.json` | Teams app manifest |
| `CustomEngineAgent/M365Agent/appPackage/color.png` | App icon (color) |
| `CustomEngineAgent/M365Agent/appPackage/outline.png` | App icon (outline) |
| `CustomEngineAgent/M365Agent/env/.env.dev` | Dev environment vars |
| `CustomEngineAgent/M365Agent/env/.env.local` | Local environment vars |
| **Infra (Azure Bicep)** | |
| `CustomEngineAgent/M365Agent/infra/azure.bicep` | Main Bicep template |
| `CustomEngineAgent/M365Agent/infra/azure.json` | Bicep config |
| `CustomEngineAgent/M365Agent/infra/azure.parameters.json` | Bicep params |
| `CustomEngineAgent/M365Agent/infra/local.azure.bicep` | Local Bicep template |
| `CustomEngineAgent/M365Agent/infra/local.azure.parameters.json` | Local Bicep params |
| `CustomEngineAgent/M365Agent/infra/entra/entra.bot.manifest.json` | Entra ID bot app registration |
| `CustomEngineAgent/M365Agent/infra/entra/entra.graph.manifest.json` | Entra ID Graph permissions |
| `CustomEngineAgent/M365Agent/infra/modules/botservice.bicep` | Azure Bot Service module |
| `CustomEngineAgent/M365Agent/infra/modules/identity.bicep` | Managed Identity module |
| `CustomEngineAgent/M365Agent/infra/modules/keyvault.bicep` | Key Vault module |
| `CustomEngineAgent/M365Agent/infra/modules/monitoring.bicep` | App Insights module |
| `CustomEngineAgent/M365Agent/infra/modules/storage.bicep` | Storage Account module |
| `CustomEngineAgent/M365Agent/infra/modules/webapp.bicep` | App Service module |

## Total: 99 files across 4 templates + 5 docs
