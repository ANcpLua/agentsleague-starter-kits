using System.Text.Json;

namespace CertPrepAgents.Middleware;

internal static class ContentSafetyCheck
{
    private static readonly HashSet<string> BlockedPatterns =
    [
        "ignore previous instructions",
        "ignore your instructions",
        "disregard your system",
        "you are now",
        "pretend you are",
        "act as if you",
        "jailbreak",
        "bypass"
    ];

    public static bool IsInputSafe(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        var normalized = input.ToLowerInvariant();
        foreach (var pattern in BlockedPatterns)
        {
            if (normalized.Contains(pattern, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    public static string GetBlockedResponse() =>
        "I can only help with Microsoft certification preparation questions. Please ask about a specific certification, study plan, or assessment.";
}

internal sealed class ContentSafetyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Only inspect POST requests to API endpoints
        if (context.Request.Method == HttpMethods.Post &&
            context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Extract user messages from OpenAI-compatible request format
            if (TryExtractUserInput(body, out var userInput) &&
                !ContentSafetyCheck.IsInputSafe(userInput))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        message = ContentSafetyCheck.GetBlockedResponse(),
                        type = "content_policy_violation",
                        code = "content_filter"
                    }
                });
                return;
            }
        }

        await next(context);
    }

    private static bool TryExtractUserInput(string body, out string? userInput)
    {
        userInput = null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // OpenAI Responses API: check "input" field (string)
            if (root.TryGetProperty("input", out var input) && input.ValueKind == JsonValueKind.String)
            {
                userInput = input.GetString();
                return userInput is not null;
            }

            // OpenAI Chat Completions: check last user message in "messages" array
            if (root.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
            {
                foreach (var msg in messages.EnumerateArray())
                {
                    if (msg.TryGetProperty("role", out var role) &&
                        role.GetString() == "user" &&
                        msg.TryGetProperty("content", out var content) &&
                        content.ValueKind == JsonValueKind.String)
                    {
                        userInput = content.GetString();
                    }
                }
                return userInput is not null;
            }
        }
        catch (JsonException)
        {
            // Malformed JSON — let it through to the endpoint for proper error handling
        }

        return false;
    }
}
