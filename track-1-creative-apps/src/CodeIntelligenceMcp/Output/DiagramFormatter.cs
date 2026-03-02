using System.Text.RegularExpressions;
using ArchTools.Mermaid;

namespace ArchTools.Output;

public static partial class DiagramFormatter
{
    public static string GetMermaidBlock(string llmResponse)
    {
        var match = MermaidBlockRegex().Match(llmResponse);

        if (!match.Success)
            throw new InvalidOperationException(
                "No Mermaid block found in the language model response. Please try again.");

        return match.Value;
    }

    public static string Format(string llmResponse, string modelName, bool fixCycles)
    {
        var mermaidBlock = GetMermaidBlock(llmResponse);
        var mermaidCode = mermaidBlock.Replace("```mermaid", "").Replace("```", "").Trim();

        if (fixCycles)
        {
            var detector = new MermaidCycleDetector(mermaidCode);
            var cycles = detector.DetectCycles();
            if (cycles is not null)
            {
                var fixedCode = detector.FixCycles(cycles);
                if (fixedCode is not null)
                {
                    mermaidCode = fixedCode;
                    mermaidBlock = $"```mermaid\n{fixedCode}\n```";
                }
            }
        }

        var linkGenerator = new MermaidLinkGenerator(mermaidCode);

        return $"""
            # Architecture Diagram

            **Model**: {modelName}
            **Mermaid Live Editor**: [View]({linkGenerator.CreateViewLink()}) | [Edit]({linkGenerator.CreateEditLink()})

            {mermaidBlock}
            """;
    }

    [GeneratedRegex(@"```mermaid[\s\S]*?```")]
    private static partial Regex MermaidBlockRegex();
}
