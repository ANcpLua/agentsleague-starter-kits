using System.Reflection;
using System.Text;
using System.Text.Json;
using EnterpriseSupportAgent.AdaptiveCards;
using EnterpriseSupportAgent.Models;
using EnterpriseSupportAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using OpenAI.Chat;

namespace EnterpriseSupportAgent.Agents;

public sealed class KnowledgeAgent : IConnectedAgent
{
    private static readonly Lock InitLock = new();
    private static List<KnowledgeDocument>? _cachedDocuments;

    private static readonly string[] ObservabilityTerms =
    [
        "trace", "traces", "span", "spans", "metric", "metrics",
        "observability", "telemetry", "otlp", "otel", "opentelemetry",
        "latency", "throughput", "error rate", "duration",
        "service map", "genai", "gen_ai",
        "log record", "log records",
        "http error", "http errors", "status code",
        "mcp.qyl", "qyl"
    ];

    private readonly IChatClientService _chatClient;
    private readonly McpObservabilityService _mcpService;
    private readonly List<KnowledgeDocument> _documents;

    public KnowledgeAgent(IChatClientService chatClient, McpObservabilityService mcpService)
    {
        _chatClient = chatClient;
        _mcpService = mcpService;
        _documents = LoadDocuments();
    }

    public string Name => "KnowledgeAgent";

    public async Task<AgentResponse> HandleAsync(string input, ITurnContext turnContext, ITurnState turnState,
        CancellationToken ct)
    {
        // Route: observability questions → MCP tools, everything else → embedded KB
        if (IsObservabilityQuery(input))
        {
            var mcpResult = await HandleObservabilityQueryAsync(input, ct);
            if (mcpResult is not null)
                return mcpResult;
            // MCP unavailable or no appropriate tool — fall through to KB
        }

        return await HandleKnowledgeQueryAsync(input, ct);
    }

    private static bool IsObservabilityQuery(string input)
    {
        var lower = input.ToLowerInvariant();
        return ObservabilityTerms.Any(term => lower.Contains(term, StringComparison.Ordinal));
    }

    private async Task<AgentResponse?> HandleObservabilityQueryAsync(string input, CancellationToken ct)
    {
        // 1. Get available MCP tools
        var toolDescriptions = await _mcpService.GetToolDescriptionsAsync(ct);
        if (toolDescriptions is null)
            return null;

        // 2. Ask LLM to select tool + arguments
        var toolSelectionMessages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a tool-calling assistant. Given the user's observability question and the available MCP tools, " +
                "select the best tool and provide arguments as JSON.\n\n" +
                "Available tools:\n" + toolDescriptions + "\n\n" +
                """
                Respond with ONLY a JSON object:
                {"tool": "tool_name", "arguments": {"param1": "value1"}}

                If no tool is appropriate, respond with:
                {"tool": "none"}
                """),
            new UserChatMessage(input)
        };

        var toolCallRaw = await CompleteChatAsync(toolSelectionMessages, ct);

        // 3. Parse tool selection
        var (toolName, arguments) = ParseToolCall(toolCallRaw);
        if (toolName is null or "none")
            return null;

        // 4. Call MCP tool
        var toolResult = await _mcpService.CallToolAsync(toolName, arguments, ct);
        if (toolResult is null)
            return null;

        // 5. Generate answer from tool results
        var answerMessages = new List<ChatMessage>
        {
            new SystemChatMessage($"""
                                   You are an enterprise observability assistant. The user asked about their system's
                                   telemetry data. You called the "{toolName}" tool and got the following result.

                                   Provide a clear, helpful answer based on this data. Summarize key findings.
                                   Highlight any anomalies, errors, or notable patterns.

                                   Tool result:
                                   {toolResult}
                                   """),
            new UserChatMessage(input)
        };

        var answer = await CompleteChatAsync(answerMessages, ct);

        // 6. Build Adaptive Card
        var card = AdaptiveCardHelper.CreateResultCard(
            "Observability Insight",
            input.Length > 60 ? input[..60] + "..." : input,
            "Live Data",
            "Good",
            answer,
            [("Source", "mcp.qyl.info"), ("Tool", toolName), ("", "")]);

        return new AgentResponse(answer, card);
    }

    private static (string? tool, Dictionary<string, object?>? args) ParseToolCall(string llmOutput)
    {
        var json = llmOutput.Trim();

        // Strip markdown code fences if present
        if (json.StartsWith("```"))
        {
            var lines = json.Split('\n');
            json = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
        }

        // Find the JSON object boundaries
        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}');
        if (start < 0 || end <= start)
            return (null, null);
        json = json[start..(end + 1)];

        using var doc = JsonDocument.Parse(json);
        var tool = doc.RootElement.GetProperty("tool").GetString();

        Dictionary<string, object?>? arguments = null;
        if (doc.RootElement.TryGetProperty("arguments", out var argsElement) &&
            argsElement.ValueKind == JsonValueKind.Object)
        {
            arguments = new Dictionary<string, object?>();
            foreach (var prop in argsElement.EnumerateObject())
            {
                arguments[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number when prop.Value.TryGetInt64(out var l) => l,
                    JsonValueKind.Number => prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => prop.Value.GetRawText()
                };
            }
        }

        return (tool, arguments);
    }

    private async Task<AgentResponse> HandleKnowledgeQueryAsync(string input, CancellationToken ct)
    {
        var topMatches = FindRelevantDocuments(input, 5);

        var context = string.Join("\n\n---\n\n", topMatches.Select(m =>
            $"**Source: {m.Source}**\n{m.Excerpt}"));

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage($"""
                                   You are an enterprise IT support knowledge assistant. Answer the user's question using ONLY
                                   the provided context. If the answer is not in the context, say "I don't have information about that
                                   in the knowledge base." Always cite sources using [Source Name] format.

                                   Context:
                                   {context}
                                   """),
            new UserChatMessage(input)
        };

        var answer = await CompleteChatAsync(messages, ct);

        var factsList = topMatches.Take(3)
            .Select(c => (c.Source, c.Excerpt.Length > 80 ? c.Excerpt[..80] + "..." : c.Excerpt)).ToList();
        while (factsList.Count < 3)
            factsList.Add(("", ""));

        var card = AdaptiveCardHelper.CreateResultCard(
            "Knowledge Base Result",
            input.Length > 60 ? input[..60] + "..." : input,
            topMatches.Count > 0 ? "Found" : "No Results",
            topMatches.Count > 0 ? "Good" : "Warning",
            answer,
            factsList);

        return new AgentResponse(answer, card);
    }

    private List<KnowledgeMatch> FindRelevantDocuments(string query, int limit)
    {
        var queryTerms = Tokenize(query);

        var scored = _documents
            .Select(doc =>
            {
                var docTerms = Tokenize(doc.Content);
                var score = queryTerms.Sum(qt =>
                    docTerms.Count(dt => dt.Contains(qt, StringComparison.OrdinalIgnoreCase)));
                return new { Document = doc, Score = score };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();

        return scored.Select(s =>
        {
            var excerpt = FindBestExcerpt(s.Document.Content, queryTerms);
            return new KnowledgeMatch
            {
                Source = s.Document.Name,
                Excerpt = excerpt
            };
        }).ToList();
    }

    private static string FindBestExcerpt(string content, string[] queryTerms)
    {
        var paragraphs = content.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries);

        var best = paragraphs
            .Select(p =>
            {
                var score = queryTerms.Count(qt =>
                    p.Contains(qt, StringComparison.OrdinalIgnoreCase));
                return new { Paragraph = p, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        var excerpt = best?.Paragraph ?? paragraphs.FirstOrDefault() ?? content;
        return excerpt.Length > 500 ? excerpt[..500] + "..." : excerpt;
    }

    private static string[] Tokenize(string text)
    {
        return text.ToLowerInvariant()
            .Split(
                [
                    ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}', '-',
                    '/'
                ],
                StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2)
            .Distinct()
            .ToArray();
    }

    private async Task<string> CompleteChatAsync(List<ChatMessage> messages, CancellationToken ct)
    {
        var response = _chatClient.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);
        return result.ToString();
    }

    private static List<KnowledgeDocument> LoadDocuments()
    {
        lock (InitLock)
        {
            if (_cachedDocuments is not null)
                return _cachedDocuments;

            var documents = new List<KnowledgeDocument>();
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

            foreach (var resourceName in resourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null) continue;

                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                var parts = resourceName.Split('.');
                var name = parts.Length >= 3
                    ? string.Join("/", parts.Skip(2).Take(parts.Length - 3))
                    : resourceName;

                documents.Add(new KnowledgeDocument
                {
                    Name = name,
                    Content = content,
                    ResourceName = resourceName
                });
            }

            _cachedDocuments = documents;
            return documents;
        }
    }

    private sealed record KnowledgeDocument
    {
        public required string Name { get; init; }
        public required string Content { get; init; }
        public required string ResourceName { get; init; }
    }

    private sealed record KnowledgeMatch
    {
        public required string Source { get; init; }
        public required string Excerpt { get; init; }
    }
}
