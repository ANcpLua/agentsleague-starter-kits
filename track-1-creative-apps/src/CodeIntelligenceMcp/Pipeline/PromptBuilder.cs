using Microsoft.Extensions.AI;

namespace ArchTools.Pipeline;

public static class PromptBuilder
{
    private const string FileSeparator = "==========";

    public static List<ChatMessage> Build(IReadOnlyList<SourceFile> files)
    {
        var fileContents = string.Join("\n", files.Select(EncodeFile));
        return
        [
            GetSystemPrompt(),
            new(ChatRole.User, fileContents)
        ];
    }

    public static ChatMessage GetSystemPrompt()
    {
        return new ChatMessage(ChatRole.System,
            $$"""
            You are an expert software engineer and software architect.

            Given code files of a repository, you need to learn the architecture of the system
            and create a high-level architecture diagram explaining it.

            Rules:
            1. You should output the diagram in mermaid.js syntax.
            2. Use the subgraph functionality in mermaid.js to add depth to the diagram.
            3. Avoid naming a subgraph and a node within it with the same name to prevent cycles.
            4. Avoid the following characters in the output: "{}:()"
            5. The architecture should be high-level and not too detailed.
            6. The output should include only the diagram, and not any additional text or explanations.

            The code files are given in the following format: full path, newline, file content.
            Different files will be separated with '{{FileSeparator}}'.
            """);
    }

    public static string EncodeFile(SourceFile file)
    {
        return $"{file.Path}\n{file.Content}{FileSeparator}";
    }
}
