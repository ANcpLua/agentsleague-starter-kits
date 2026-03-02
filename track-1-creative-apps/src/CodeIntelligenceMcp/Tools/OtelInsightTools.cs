using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

internal sealed class OtelInsightTools
{
    [McpServerTool]
    [Description("Analyzes OpenTelemetry trace data (as JSON) and returns a human-readable narrative explaining what happened, why it was slow, and what to fix. Transforms raw span data into actionable insights. Use when a user shares trace/span JSON and asks 'why is this slow' or 'what happened'.")]
    public static async Task<string> ExplainTrace(
        [Description("OpenTelemetry trace data as JSON string. Can be a single span, array of spans, or full trace export.")] string traceJson,
        [Description("What to focus on: 'latency' for performance analysis, 'errors' for failure diagnosis, 'flow' for request path visualization, or 'all'.")] string focus,
        IChatClient chatClient)
    {
        using var activity = McpActivitySource.StartToolActivity("explain_trace", "gpt-4o-mini",
            new() { ["code_intel.focus"] = focus });

        var prompt = $"""
            You are an observability expert who makes complex distributed traces understandable.

            Analyze this OpenTelemetry trace data with focus on: {focus}

            ```json
            {traceJson}
            ```

            Provide your analysis as a narrative (not just data):

            ## What Happened
            [Human-readable story of the request flow across services]

            ## Performance Analysis
            [Identify the slowest operations, p99 contributors, and bottlenecks]

            ## Issues Found
            [Errors, retries, timeouts, or anomalies detected in the spans]

            ## Recommendations
            [Specific, actionable fixes ranked by impact]
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        activity?.SetTag("gen_ai.usage.output_tokens", response.Usage?.OutputTokenCount);
        return response.Text;
    }
}
