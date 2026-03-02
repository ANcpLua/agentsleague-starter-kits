using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Azure.Identity;
using GitHub.Copilot.SDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using OpenTelemetry;
using OpenTelemetry.Trace;

return await App.RunAsync(args);

internal static class App
{
    public static async Task<int> RunAsync(string[] args)
    {
        CliOptions options;
        try
        {
            options = CliOptions.Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            CliOptions.PrintHelp();
            return 2;
        }

        if (options.ShowHelp)
        {
            CliOptions.PrintHelp();
            return 0;
        }

        using TracerProvider? tracerProvider = BuildTelemetry(options);

        await using IAgentProvider provider = await AgentProviderFactory.CreateAsync(options);

        IReadOnlyList<AITool> tools = ToolFactory.Create(options.WorkingDirectory);

        AIAgent simplifierAgent = await provider.CreateAgentAsync(
            name: "CodeSimplifier",
            description: "Refactors code for readability while preserving behavior.",
            instructions: Prompts.Simplifier,
            tools: tools);

        AIAgent reviewerAgent = await provider.CreateAgentAsync(
            name: "CodeReviewer",
            description: "Rejects risky simplifications and enforces behavioral equivalence.",
            instructions: Prompts.Reviewer,
            tools: tools);

        if (options.EnableOtel)
        {
            simplifierAgent = simplifierAgent.AsBuilder()
                .UseOpenTelemetry("full-code-simplifier.simplifier")
                .Build();

            reviewerAgent = reviewerAgent.AsBuilder()
                .UseOpenTelemetry("full-code-simplifier.reviewer")
                .Build();
        }

        if (options.Mode == RunMode.ServeAgui)
        {
            await RunAguiAsync(options, simplifierAgent, reviewerAgent);
            return 0;
        }

        return await RunSimplifierAsync(options, simplifierAgent, reviewerAgent);
    }

    private static async Task<int> RunSimplifierAsync(CliOptions options, AIAgent simplifierAgent, AIAgent reviewerAgent)
    {
        if (string.IsNullOrWhiteSpace(options.TargetFile))
        {
            throw new ArgumentException("--file is required for simplify mode.");
        }

        string filePath = Path.GetFullPath(options.TargetFile, options.WorkingDirectory);
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Target file not found: {filePath}");
            return 2;
        }

        string originalCode = await File.ReadAllTextAsync(filePath);
        if (Encoding.UTF8.GetByteCount(originalCode) > options.MaxInputBytes)
        {
            Console.Error.WriteLine($"Input file is larger than --max-input-bytes ({options.MaxInputBytes}).");
            return 2;
        }

        string simplifierPrompt = PromptBuilder.BuildSimplifierPrompt(filePath, originalCode, options);
        string candidateResponse;

        if (options.UseWorkflow)
        {
            candidateResponse = await WorkflowPipeline.RunAsync(
                simplifierAgent,
                reviewerAgent,
                simplifierPrompt,
                options.StreamWorkflowEvents);
        }
        else
        {
            AgentSession simplifierSession = await simplifierAgent.CreateSessionAsync();
            AgentResponse simplifierResponse = await simplifierAgent.RunAsync(simplifierPrompt, simplifierSession);
            string candidateCode = ResponseParser.ExtractCode(simplifierResponse.Text, "simplified_code");

            if (string.IsNullOrWhiteSpace(candidateCode))
            {
                Console.Error.WriteLine("Simplifier agent did not return <simplified_code>...</simplified_code>.");
                return 1;
            }

            AgentSession reviewerSession = await reviewerAgent.CreateSessionAsync();
            string reviewerPrompt = PromptBuilder.BuildReviewerPrompt(filePath, originalCode, candidateCode);
            AgentResponse reviewerResponse = await reviewerAgent.RunAsync(reviewerPrompt, reviewerSession);
            candidateResponse = reviewerResponse.Text;
        }

        string finalCode = ResponseParser.ExtractCode(candidateResponse, "final_code");
        if (string.IsNullOrWhiteSpace(finalCode))
        {
            finalCode = ResponseParser.ExtractCode(candidateResponse, "simplified_code");
        }

        if (string.IsNullOrWhiteSpace(finalCode))
        {
            Console.Error.WriteLine("Reviewer output did not include <final_code> or <simplified_code> tags.");
            return 1;
        }

        if (NormalizeNewlines(originalCode) == NormalizeNewlines(finalCode))
        {
            Console.WriteLine("No behavioral-safe simplification changes were produced.");
            return 0;
        }

        string diff = await DiffUtility.CreateDiffAsync(filePath, originalCode, finalCode);
        Console.WriteLine(diff);

        if (options.Apply)
        {
            if (options.CreateBackup)
            {
                string backupPath = filePath + ".bak";
                await File.WriteAllTextAsync(backupPath, originalCode, Encoding.UTF8);
                Console.WriteLine($"Backup written: {backupPath}");
            }

            await File.WriteAllTextAsync(filePath, finalCode, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"Applied simplification to: {filePath}");
        }

        string reportPath = await ReportWriter.WriteReportAsync(options.WorkingDirectory, filePath, diff, candidateResponse, options);
        Console.WriteLine($"Report: {reportPath}");

        return 0;
    }

    private static async Task RunAguiAsync(CliOptions options, AIAgent simplifierAgent, AIAgent reviewerAgent)
    {
        Workflow workflow = AgentWorkflowBuilder.BuildSequential("code-simplifier-workflow", new[]
        {
            simplifierAgent,
            reviewerAgent,
        });

        AIAgent hostedAgent = workflow.AsAIAgent(
            name: "Code Simplifier Workflow",
            description: "Two-stage simplifier and reviewer workflow",
            includeWorkflowOutputsInResponse: true);

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAGUI();
        WebApplication app = builder.Build();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", mode = "ag-ui" }));
        app.MapAGUI("/", hostedAgent);

        Console.WriteLine($"AG-UI endpoint: {options.AguiUrl}");
        await app.RunAsync(options.AguiUrl);
    }

    private static TracerProvider? BuildTelemetry(CliOptions options)
    {
        if (!options.EnableOtel)
        {
            return null;
        }

        return Sdk.CreateTracerProviderBuilder()
            .AddSource("*Microsoft.Agents.AI")
            .AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(options.OtlpEndpoint);
            })
            .Build();
    }

    private static string NormalizeNewlines(string input)
    {
        return input.Replace("\r\n", "\n");
    }
}

internal static class WorkflowPipeline
{
    public static async Task<string> RunAsync(
        AIAgent simplifierAgent,
        AIAgent reviewerAgent,
        string input,
        bool streamEvents,
        CancellationToken cancellationToken = default)
    {
        Workflow workflow = AgentWorkflowBuilder.BuildSequential("code-simplifier-seq", new[]
        {
            simplifierAgent,
            reviewerAgent,
        });

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, input, cancellationToken: cancellationToken);

        StringBuilder allText = new();

        await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
        {
            if (evt is AgentResponseUpdateEvent updateEvent)
            {
                string text = updateEvent.Update.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    allText.Append(text);
                    if (streamEvents)
                    {
                        Console.Write(text);
                    }
                }
            }
        }

        if (streamEvents)
        {
            Console.WriteLine();
        }

        return allText.ToString();
    }
}

internal static class ToolFactory
{
    public static IReadOnlyList<AITool> Create(string workspaceRoot)
    {
        AIFunction readFile = AIFunctionFactory.Create(
            (string relativePath, int startLine, int lineCount) => ReadFileWindow(workspaceRoot, relativePath, startLine, lineCount),
            "ReadFileWindow",
            "Read a text file window for code analysis. Input path is relative to workspace root.");

        AIFunction listDirectory = AIFunctionFactory.Create(
            (string relativePath, int maxEntries) => ListDirectory(workspaceRoot, relativePath, maxEntries),
            "ListDirectory",
            "List files and directories under a path relative to workspace root.");

        AIFunction searchText = AIFunctionFactory.Create(
            (string relativePath, string searchText, int maxMatches) => SearchText(workspaceRoot, relativePath, searchText, maxMatches),
            "SearchTextInFile",
            "Search plain text occurrences in a file and return line matches.");

        return new AITool[] { readFile, listDirectory, searchText };
    }

    private static string ReadFileWindow(string workspaceRoot, string relativePath, int startLine, int lineCount)
    {
        string path = ResolvePath(workspaceRoot, relativePath);
        string[] lines = File.ReadAllLines(path);

        int start = Math.Max(1, startLine);
        int count = Math.Max(1, Math.Min(lineCount, 500));
        int startIndex = Math.Min(start - 1, lines.Length);

        StringBuilder sb = new();
        for (int i = startIndex; i < Math.Min(lines.Length, startIndex + count); i++)
        {
            sb.AppendLine($"{i + 1,6}: {lines[i]}");
        }

        return sb.ToString();
    }

    private static string ListDirectory(string workspaceRoot, string relativePath, int maxEntries)
    {
        string path = ResolvePath(workspaceRoot, relativePath);
        int cap = Math.Max(1, Math.Min(maxEntries, 500));

        IEnumerable<string> entries = Directory
            .EnumerateFileSystemEntries(path)
            .OrderBy(x => x)
            .Take(cap)
            .Select(x => Path.GetRelativePath(workspaceRoot, x));

        return string.Join(Environment.NewLine, entries);
    }

    private static string SearchText(string workspaceRoot, string relativePath, string searchText, int maxMatches)
    {
        string path = ResolvePath(workspaceRoot, relativePath);
        int cap = Math.Max(1, Math.Min(maxMatches, 200));

        string[] lines = File.ReadAllLines(path);
        StringBuilder sb = new();
        int found = 0;

        for (int i = 0; i < lines.Length && found < cap; i++)
        {
            if (lines[i].Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"{i + 1,6}: {lines[i]}");
                found++;
            }
        }

        return sb.ToString();
    }

    private static string ResolvePath(string workspaceRoot, string relativePath)
    {
        string full = Path.GetFullPath(relativePath, workspaceRoot);
        string root = Path.GetFullPath(workspaceRoot);

        if (!full.StartsWith(root, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path escapes workspace root.");
        }

        if (!File.Exists(full) && !Directory.Exists(full))
        {
            throw new FileNotFoundException("Path not found.", full);
        }

        return full;
    }
}

internal static class PromptBuilder
{
    public static string BuildSimplifierPrompt(string filePath, string sourceCode, CliOptions options)
    {
        string languageHint = Path.GetExtension(filePath).TrimStart('.');

        return $$"""
You are stage 1 of a two-stage simplification pipeline.

Goal:
- Simplify code for readability, maintainability, and explicitness.
- Preserve exact runtime behavior.
- Do not remove error handling.
- Do not change public APIs, signatures, or serialization contracts.
- Avoid nested ternary operators.

Output contract:
- Return ONLY one XML block: <simplified_code>...</simplified_code>
- The block MUST contain a full-file replacement.
- No markdown code fences.

Context:
- File: {{filePath}}
- Language hint: {{languageHint}}
- Apply level: {{options.SimplificationLevel}}

Source file:
<source_code>
{{sourceCode}}
</source_code>
""";
    }

    public static string BuildReviewerPrompt(string filePath, string originalCode, string simplifiedCode)
    {
        return $$"""
You are stage 2 reviewer.

Validate candidate simplification against the original.
Reject risky changes. Keep behavior equivalent.

Checks:
- Runtime behavior unchanged
- Public API unchanged
- Error semantics preserved
- Nullability and resource disposal preserved

Output contract:
- First line: REVIEW_RESULT: APPROVED or REVIEW_RESULT: REJECTED
- Then one XML block with full final code: <final_code>...</final_code>
- No markdown fences.

File: {{filePath}}

<original_code>
{{originalCode}}
</original_code>

<candidate_code>
{{simplifiedCode}}
</candidate_code>
""";
    }
}

internal static class ResponseParser
{
    public static string ExtractCode(string response, string tag)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return string.Empty;
        }

        string pattern = $"<{tag}>(.*?)</{tag}>";
        Match match = Regex.Match(response, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        Match fenceMatch = Regex.Match(response, "```(?:[a-zA-Z0-9_+-]*)\\s*(.*?)```", RegexOptions.Singleline);
        if (fenceMatch.Success)
        {
            return fenceMatch.Groups[1].Value.Trim();
        }

        return string.Empty;
    }
}

internal static class DiffUtility
{
    public static async Task<string> CreateDiffAsync(string path, string oldText, string newText)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "full-code-simplifier", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        string oldFile = Path.Combine(tempDir, "old.txt");
        string newFile = Path.Combine(tempDir, "new.txt");

        await File.WriteAllTextAsync(oldFile, oldText);
        await File.WriteAllTextAsync(newFile, newText);

        ProcessStartInfo psi = new()
        {
            FileName = "git",
            Arguments = $"diff --no-index --unified=5 -- {Quote(oldFile)} {Quote(newFile)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using Process? process = Process.Start(psi);
        if (process is null)
        {
            return "Failed to start git diff process.";
        }

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        string header = $"# Diff for {path}{Environment.NewLine}";

        return process.ExitCode switch
        {
            0 => header + "No changes.",
            1 => header + stdout,
            _ => header + "git diff failed:" + Environment.NewLine + stderr,
        };
    }

    private static string Quote(string value)
    {
        return '"' + value.Replace("\"", "\\\"") + '"';
    }
}

internal static class ReportWriter
{
    public static async Task<string> WriteReportAsync(string workspace, string filePath, string diff, string rawAgentOutput, CliOptions options)
    {
        string reportsDir = Path.Combine(workspace, "artifacts", "full-code-simplifier");
        Directory.CreateDirectory(reportsDir);

        string stamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
        string reportPath = Path.Combine(reportsDir, $"report-{stamp}.md");

        string content = $$"""
# Full Code Simplifier Report

- File: `{{filePath}}`
- Provider: `{{options.Provider}}`
- Workflow mode: `{{options.UseWorkflow}}`
- Apply: `{{options.Apply}}`
- Timestamp: `{{DateTimeOffset.Now:O}}`

## Diff

```diff
{{diff}}
```

## Raw Agent Output

```text
{{rawAgentOutput}}
```
""";

        await File.WriteAllTextAsync(reportPath, content);
        return reportPath;
    }
}

internal static class Prompts
{
    public const string Simplifier = """
You are an elite senior engineer focused on safe code simplification.
You simplify structure and naming while preserving behavior exactly.
Prefer explicit readable code over compact code golf.
""";

    public const string Reviewer = """
You are a strict reviewer validating behavior-preserving refactors.
Reject hidden behavior changes.
Return the safest final code.
""";
}

internal interface IAgentProvider : IAsyncDisposable
{
    Task<AIAgent> CreateAgentAsync(string name, string description, string instructions, IReadOnlyList<AITool> tools);
}

internal static class AgentProviderFactory
{
    public static async Task<IAgentProvider> CreateAsync(CliOptions options)
    {
        return options.Provider switch
        {
            ProviderKind.Copilot => await CopilotProvider.CreateAsync(options),
            ProviderKind.AzureOpenAi => new AzureOpenAiProvider(options),
            ProviderKind.GitHubModels => new GitHubModelsProvider(options),
            _ => throw new InvalidOperationException($"Unsupported provider: {options.Provider}"),
        };
    }
}

internal sealed class CopilotProvider : IAgentProvider
{
    private readonly CopilotClient _copilotClient;
    private readonly CliOptions _options;

    private CopilotProvider(CopilotClient copilotClient, CliOptions options)
    {
        _copilotClient = copilotClient;
        _options = options;
    }

    public static async Task<CopilotProvider> CreateAsync(CliOptions options)
    {
        CopilotClient client = new();
        await client.StartAsync();
        return new CopilotProvider(client, options);
    }

    public Task<AIAgent> CreateAgentAsync(string name, string description, string instructions, IReadOnlyList<AITool> tools)
    {
        SessionConfig sessionConfig = new()
        {
            Model = string.IsNullOrWhiteSpace(_options.Model) ? null : _options.Model,
            WorkingDirectory = _options.WorkingDirectory,
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Replace,
                Content = instructions,
            },
            Tools = tools.OfType<AIFunction>().ToList(),
            OnPermissionRequest = PermissionPrompts.PromptPermission,
            InfiniteSessions = new InfiniteSessionConfig
            {
                Enabled = true,
            },
        };

        if (_options.EnableMcp)
        {
            sessionConfig.McpServers = new Dictionary<string, object>
            {
                ["filesystem"] = new McpLocalServerConfig
                {
                    Type = "stdio",
                    Command = "npx",
                    Args = new List<string> { "-y", "@modelcontextprotocol/server-filesystem", "." },
                    Tools = new List<string> { "*" },
                    Cwd = _options.WorkingDirectory,
                },
                ["microsoft-learn"] = new McpRemoteServerConfig
                {
                    Type = "http",
                    Url = "https://learn.microsoft.com/api/mcp",
                    Tools = new List<string> { "*" },
                },
            };
        }

        AIAgent agent = _copilotClient.AsAIAgent(
            sessionConfig: sessionConfig,
            name: name,
            description: description);

        return Task.FromResult(agent);
    }

    public ValueTask DisposeAsync()
    {
        return _copilotClient.DisposeAsync();
    }
}

internal sealed class AzureOpenAiProvider : IAgentProvider
{
    private readonly CliOptions _options;

    public AzureOpenAiProvider(CliOptions options)
    {
        _options = options;
    }

    public Task<AIAgent> CreateAgentAsync(string name, string description, string instructions, IReadOnlyList<AITool> tools)
    {
        string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT.");

        string model = _options.Model
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL")
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
            ?? "gpt-4.1";

#pragma warning disable OPENAI001
        OpenAIClient client = new(
            new BearerTokenPolicy(new AzureCliCredential(), "https://ai.azure.com/.default"),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
            });

        ResponsesClient responsesClient = client.GetResponsesClient();
        AIAgent agent = responsesClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = name,
                Description = description,
                ChatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    ModelId = model,
                    Tools = tools.ToList(),
                },
            });
#pragma warning restore OPENAI001

        return Task.FromResult(agent);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal sealed class GitHubModelsProvider : IAgentProvider
{
    private readonly CliOptions _options;

    public GitHubModelsProvider(CliOptions options)
    {
        _options = options;
    }

    public Task<AIAgent> CreateAgentAsync(string name, string description, string instructions, IReadOnlyList<AITool> tools)
    {
        string token = Environment.GetEnvironmentVariable("GITHUB_MODELS_TOKEN")
            ?? throw new InvalidOperationException("Set GITHUB_MODELS_TOKEN.");

        string endpoint = Environment.GetEnvironmentVariable("GITHUB_MODELS_ENDPOINT")
            ?? "https://models.inference.ai.azure.com";

        string model = _options.Model
            ?? Environment.GetEnvironmentVariable("GITHUB_MODELS_MODEL")
            ?? "gpt-4.1";

        OpenAIClient client = new(
            new ApiKeyCredential(token),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint),
            });

        ChatClient chatClient = client.GetChatClient(model);
        AIAgent agent = chatClient.AsAIAgent(
            name: name,
            description: description,
            instructions: instructions,
            tools: tools.ToList());

        return Task.FromResult(agent);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal static class PermissionPrompts
{
    public static Task<PermissionRequestResult> PromptPermission(PermissionRequest request, PermissionInvocation invocation)
    {
        Console.WriteLine();
        Console.WriteLine($"[Permission Request] kind={request.Kind}, toolCallId={request.ToolCallId}");
        Console.Write("Approve? (y/n): ");

        string? input = Console.ReadLine()?.Trim();
        bool approved = string.Equals(input, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(new PermissionRequestResult
        {
            Kind = approved ? "approved" : "denied-interactively-by-user",
        });
    }
}

internal enum ProviderKind
{
    Copilot,
    AzureOpenAi,
    GitHubModels,
}

internal enum RunMode
{
    Simplify,
    ServeAgui,
}

internal sealed class CliOptions
{
    public RunMode Mode { get; private set; } = RunMode.Simplify;

    public ProviderKind Provider { get; private set; } = ProviderKind.Copilot;

    public string? TargetFile { get; private set; }

    public bool Apply { get; private set; }

    public bool CreateBackup { get; private set; } = true;

    public bool UseWorkflow { get; private set; } = true;

    public bool StreamWorkflowEvents { get; private set; }

    public bool EnableMcp { get; private set; }

    public bool EnableOtel { get; private set; } = IsEnvTrue("ENABLE_OTEL");

    public string OtlpEndpoint { get; private set; } = Environment.GetEnvironmentVariable("OTLP_ENDPOINT") ?? "http://localhost:4317";

    public string? Model { get; private set; }

    public int MaxInputBytes { get; private set; } = 200_000;

    public string SimplificationLevel { get; private set; } = "balanced";

    public string WorkingDirectory { get; private set; } = Directory.GetCurrentDirectory();

    public string AguiUrl { get; private set; } = "http://localhost:50516";

    public bool ShowHelp { get; private set; }

    public static CliOptions Parse(string[] args)
    {
        CliOptions options = new();

        List<string> tokens = args.ToList();
        if (tokens.Count > 0 && !tokens[0].StartsWith("--", StringComparison.Ordinal))
        {
            string mode = tokens[0].ToLowerInvariant();
            tokens.RemoveAt(0);
            options.Mode = mode switch
            {
                "simplify" => RunMode.Simplify,
                "serve-agui" => RunMode.ServeAgui,
                "help" => RunMode.Simplify,
                _ => throw new ArgumentException($"Unknown mode: {mode}"),
            };

            if (mode == "help")
            {
                options.ShowHelp = true;
                return options;
            }
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            string arg = tokens[i];
            switch (arg)
            {
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                case "--file":
                    options.TargetFile = NextValue(tokens, ref i, arg);
                    break;
                case "--provider":
                    options.Provider = ParseProvider(NextValue(tokens, ref i, arg));
                    break;
                case "--apply":
                    options.Apply = true;
                    break;
                case "--no-backup":
                    options.CreateBackup = false;
                    break;
                case "--use-workflow":
                    options.UseWorkflow = true;
                    break;
                case "--no-workflow":
                    options.UseWorkflow = false;
                    break;
                case "--stream":
                    options.StreamWorkflowEvents = true;
                    break;
                case "--enable-mcp":
                    options.EnableMcp = true;
                    break;
                case "--enable-otel":
                    options.EnableOtel = true;
                    break;
                case "--otlp-endpoint":
                    options.OtlpEndpoint = NextValue(tokens, ref i, arg);
                    break;
                case "--model":
                    options.Model = NextValue(tokens, ref i, arg);
                    break;
                case "--max-input-bytes":
                    options.MaxInputBytes = int.Parse(NextValue(tokens, ref i, arg));
                    break;
                case "--simplification-level":
                    options.SimplificationLevel = NextValue(tokens, ref i, arg);
                    break;
                case "--workdir":
                    options.WorkingDirectory = Path.GetFullPath(NextValue(tokens, ref i, arg));
                    break;
                case "--agui-url":
                    options.AguiUrl = NextValue(tokens, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {arg}");
            }
        }

        return options;
    }

    public static void PrintHelp()
    {
        Console.WriteLine(
            """
Full Code Simplifier (Microsoft Agent Framework + GitHub Copilot SDK)

Modes:
  simplify     Run two-stage simplification/review against a file (default)
  serve-agui   Host the simplifier workflow via AG-UI endpoint

Usage:
  dotnet run -- simplify --file <path> [options]
  dotnet run -- serve-agui [options]

Common options:
  --provider <copilot|azure-openai|github-models>
  --model <model-or-deployment>
  --enable-mcp
  --enable-otel
  --otlp-endpoint <url>
  --workdir <path>

Simplify mode options:
  --file <path>
  --apply
  --no-backup
  --use-workflow / --no-workflow
  --stream
  --simplification-level <light|balanced|aggressive>
  --max-input-bytes <n>

AG-UI mode options:
  --agui-url <url>   (default: http://localhost:50516)

Environment variables:
  AZURE_OPENAI_ENDPOINT
  AZURE_OPENAI_MODEL or AZURE_OPENAI_DEPLOYMENT_NAME
  GITHUB_MODELS_TOKEN
  GITHUB_MODELS_ENDPOINT
  GITHUB_MODELS_MODEL
  ENABLE_OTEL=true
  OTLP_ENDPOINT=http://localhost:4317
""");
    }

    private static ProviderKind ParseProvider(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "copilot" => ProviderKind.Copilot,
            "azure-openai" => ProviderKind.AzureOpenAi,
            "github-models" => ProviderKind.GitHubModels,
            _ => throw new ArgumentException($"Unsupported provider: {value}"),
        };
    }

    private static string NextValue(IReadOnlyList<string> args, ref int index, string option)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"Missing value for {option}");
        }

        index++;
        return args[index];
    }

    private static bool IsEnvTrue(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
