using System.Diagnostics;

internal static class McpActivitySource
{
    public const string SourceName = "code-intelligence-mcp";
    private static readonly ActivitySource Source = new(SourceName, "1.0.0");

    public static Activity? StartToolActivity(string toolName, string model, Dictionary<string, string>? tags = null)
    {
        var activity = Source.StartActivity($"mcp.tool.{toolName}", ActivityKind.Internal);
        activity?.SetTag("gen_ai.operation.name", toolName);
        activity?.SetTag("gen_ai.request.model", model);
        activity?.SetTag("code_intel.tool", toolName);
        if (tags is not null)
            foreach (var (key, value) in tags)
                activity?.SetTag(key, value);
        return activity;
    }
}
