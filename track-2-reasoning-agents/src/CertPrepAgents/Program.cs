using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using CertPrepAgents.Contracts;
using CertPrepAgents.Middleware;
using CertPrepAgents.Prompts;
using CertPrepAgents.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- Chat clients: standard + reasoning ---
var credential = new ApiKeyCredential(
    builder.Configuration["GITHUB_TOKEN"]
    ?? throw new InvalidOperationException("Missing configuration: GITHUB_TOKEN. Use 'dotnet user-secrets set GITHUB_TOKEN <token>'."));

var githubModelsOptions = new OpenAIClientOptions
{
    Endpoint = new Uri("https://models.inference.ai.azure.com")
};

var standardClient = new ChatClient("gpt-4o-mini", credential, githubModelsOptions)
    .AsIChatClient();

var reasoningClient = new ChatClient("o4-mini", credential, githubModelsOptions)
    .AsIChatClient();

builder.Services.AddChatClient(standardClient);
builder.Services.AddHttpClient();

// --- Register 5 agents ---

// 1. Curator Agent — finds Microsoft Learn modules (standard model)
builder.AddAIAgent("curator", (sp, key) =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    return new ChatClientAgent(
        standardClient,
        name: key,
        instructions: CuratorPrompt.Instructions,
        tools:
        [
            AIFunctionFactory.Create(
                (string certification, string domain) => LearnSearchTool.SearchLearn(certification, domain, httpClient),
                "SearchLearn",
                "Searches Microsoft Learn for training modules related to a certification domain.")
        ]);
});

// 2. Study Plan Generator — structures modules into a study plan (standard model)
builder.AddAIAgent("study-plan-generator", StudyPlanPrompt.Instructions);

// 3. Assessment Agent — generates + grades exam questions (reasoning model)
builder.AddAIAgent("assessment", (sp, key) => new ChatClientAgent(
    reasoningClient,
    name: key,
    instructions: AssessmentPrompt.Instructions,
    tools:
    [
        AIFunctionFactory.Create(
            (string questionsJson) => GradeAnswersTool.GradeAnswers(questionsJson, reasoningClient),
            "GradeAnswers",
            "Grades student answers to certification exam questions with detailed reasoning.")
    ]));

// 4. Certification Planner — pass/fail decision with remediation (standard model)
builder.AddAIAgent("planner", PlannerPrompt.Instructions);

// 5. Engagement Agent — sends study plan/remediation email (standard model)
builder.AddAIAgent("engagement", (sp, key) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ChatClientAgent(
        standardClient,
        name: key,
        instructions: EngagementPrompt.Instructions,
        tools:
        [
            AIFunctionFactory.Create(
                (string to, string subject, string body) => SendEmailTool.SendEmail(to, subject, body, config),
                "SendEmail",
                "Sends an email with the study plan or remediation plan to the student.")
        ]);
});

// --- Assessment sub-workflow: Curator → Study Plan → Assessment → Planner ---
builder.AddWorkflow("assess", (sp, key) => AgentWorkflowBuilder.BuildSequential(
    workflowName: key,
    agents:
    [
        sp.GetRequiredKeyedService<AIAgent>("curator"),
        sp.GetRequiredKeyedService<AIAgent>("study-plan-generator"),
        sp.GetRequiredKeyedService<AIAgent>("assessment"),
        sp.GetRequiredKeyedService<AIAgent>("planner")
    ]
)).AddAsAIAgent("assess-agent");

// --- Cert-prep orchestrator with remediation loop ---
// If the planner says "Remediate", re-run the assessment sub-workflow
// with weak-domain context (up to 3 attempts). On "Pass", run Engagement.
builder.AddAIAgent("cert-prep-agent", (sp, key) =>
{
    var assessAgent = sp.GetRequiredKeyedService<AIAgent>("assess-agent");
    var engagementAgent = sp.GetRequiredKeyedService<AIAgent>("engagement");

    return new ChatClientAgent(standardClient, name: key, instructions: "")
        .AsBuilder()
        .Use(
            runFunc: async (messages, thread, options, _, ct) =>
            {
                const int maxAttempts = 3;
                var currentMessages = messages.ToList();

                for (var attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var assessResponse = await assessAgent.RunAsync(currentMessages, thread, options, ct);
                    var plannerText = assessResponse.Text;

                    bool isPass = plannerText.Contains("\"decision\":\"Pass\"", StringComparison.OrdinalIgnoreCase)
                               || plannerText.Contains("\"decision\": \"Pass\"", StringComparison.OrdinalIgnoreCase);

                    if (isPass || attempt == maxAttempts - 1)
                    {
                        return await engagementAgent.RunAsync(assessResponse.Messages, thread, options, ct);
                    }

                    // Remediate: enrich context with weak domains and loop back
                    currentMessages =
                    [
                        new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                            "The student needs remediation. Planner decision:\n" + plannerText + "\n\n" +
                            "Focus ONLY on the weak domains identified above. " +
                            "Find targeted learning resources for those specific domains.")
                    ];
                }

                throw new InvalidOperationException("Unreachable: loop should exit via return.");
            },
            runStreamingFunc: null)
        .Build();
});

// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("cert-prep-agents")
        .AddSource("*Microsoft.Agents.AI")
        .AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri(
                builder.Configuration["Otel:Endpoint"] ?? "http://localhost:4317");
        }));

// --- OpenAI Responses + Conversations (required for DevUI) ---
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();
app.UseHttpsRedirection();

// Content safety guardrail — blocks prompt injection before it reaches any agent
app.UseMiddleware<ContentSafetyMiddleware>();

// Map endpoints for OpenAI responses and conversations
app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (builder.Environment.IsDevelopment())
{
    app.MapDevUI();
}

app.Run();
