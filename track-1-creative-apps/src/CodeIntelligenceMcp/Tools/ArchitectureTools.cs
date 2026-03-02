using System.ComponentModel;
using ArchTools.Llm;
using ArchTools.Output;
using ArchTools.Pipeline;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

internal sealed class ArchitectureTools
{
    private static readonly string[] DefaultExtensions =
        ["cs", "ts", "tsx", "js", "jsx", "py", "go", "java", "rs", "kt", "rb", "php", "swift", "cpp", "c", "h"];

    private static readonly string[] DefaultExcludePatterns =
        ["**/bin/**", "**/obj/**", "**/node_modules/**", "**/.*/**"];

    [McpServerTool]
    [Description(
        "Generates a high-level Mermaid architecture diagram from source code in a directory. " +
        "Scans files, selects within a token budget, sends to an LLM, and returns formatted " +
        "Mermaid markdown with interactive mermaid.live editor links. Use when a user asks " +
        "'show me the architecture', 'generate a diagram of this project', or 'visualize this codebase'.")]
    public static async Task<string> GenerateArchitectureDiagram(
        [Description("Absolute path to the source directory to scan.")] string directoryPath,
        [Description("File extensions to include, comma-separated (e.g., 'cs,ts,py'). " +
                     "Defaults to common languages if omitted.")] string? extensions,
        [Description("Glob patterns to exclude, comma-separated (e.g., '**/test/**,**/docs/**'). " +
                     "Defaults to bin/obj/node_modules if omitted.")] string? excludePatterns,
        [Description("Maximum character budget for source content sent to the LLM. " +
                     "Defaults to 500000. Lower values produce faster but less detailed diagrams.")] int? maxChars,
        [Description("Maximum number of files to scan. Defaults to 1000.")] int? maxFiles,
        IChatClient chatClient)
    {
        using var activity = McpActivitySource.StartToolActivity(
            "generate_architecture_diagram", "gpt-4o-mini",
            new()
            {
                ["archtools.directory"] = directoryPath,
                ["archtools.max_chars"] = (maxChars ?? 500_000).ToString()
            });

        if (!Directory.Exists(directoryPath))
            return $"Error: Directory not found: {directoryPath}";

        var extList = extensions is not null
            ? extensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : DefaultExtensions.ToList();

        var excludeList = excludePatterns is not null
            ? excludePatterns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : DefaultExcludePatterns.ToList();

        // 1. Scan files
        var scanner = new FileScanner(directoryPath, extList, excludeList, maxFiles ?? 1000);
        var files = scanner.Scan();

        if (files.Count == 0)
            return "No matching source files found. Check the directory path and file extensions.";

        activity?.SetTag("archtools.files_scanned", files.Count.ToString());

        // 2. Budget
        var selected = TokenBudgeter.SelectFiles(files, maxChars ?? 500_000);
        activity?.SetTag("archtools.files_selected", selected.Count.ToString());

        // 3. Build prompt + call LLM
        var messages = PromptBuilder.Build(selected);
        string llmResponse;
        try
        {
            llmResponse = await DiagramGenerator.GenerateAsync(chatClient, messages);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetTag("error", true.ToString());
            activity?.SetTag("error.message", ex.Message);
            return $"Error generating diagram: {ex.Message}";
        }

        // 4. Format (extract mermaid, fix cycles, add links)
        string result;
        try
        {
            result = DiagramFormatter.Format(llmResponse, "gpt-4o-mini", fixCycles: true);
        }
        catch (InvalidOperationException ex)
        {
            activity?.SetTag("error", true.ToString());
            return $"Error: {ex.Message}";
        }

        activity?.SetTag("gen_ai.usage.output_tokens", llmResponse.Length / 4);

        result += $"""


            ---
            **Files scanned**: {files.Count} | **Files used**: {selected.Count} | **Character budget**: {maxChars ?? 500_000:N0}
            """;

        return result;
    }
}
