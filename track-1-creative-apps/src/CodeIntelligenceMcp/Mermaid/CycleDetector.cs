namespace ArchTools.Mermaid;

public record Subgraph(string Name, int Line);

public sealed class MermaidCycleDetector(string mermaidCode)
{
    public Dictionary<int, Subgraph>? DetectCycles()
    {
        var cyclicSubgraphs = DetectCyclicSubgraphs();
        return cyclicSubgraphs.Count > 0 ? cyclicSubgraphs : null;
    }

    private Dictionary<int, Subgraph> DetectCyclicSubgraphs()
    {
        var lines = mermaidCode.Split('\n');
        var ancestorSubgraphs = new List<Subgraph>();
        var cyclicSubgraphs = new Dictionary<int, Subgraph>();

        for (var i = 0; i < lines.Length; i++)
        {
            ProcessLine(lines[i], i, ancestorSubgraphs, cyclicSubgraphs);
        }

        return cyclicSubgraphs;
    }

    private static void ProcessLine(
        string line,
        int lineNumber,
        List<Subgraph> ancestorSubgraphs,
        Dictionary<int, Subgraph> cyclicSubgraphs)
    {
        line = line.Trim();

        if (string.IsNullOrEmpty(line))
            return;

        if (line.StartsWith("subgraph"))
        {
            var rest = line["subgraph".Length..];
            var subgraphName = rest.Split('[')[0].Trim();

            CheckIfCyclic(ancestorSubgraphs, subgraphName, cyclicSubgraphs);
            ancestorSubgraphs.Add(new Subgraph(subgraphName, lineNumber));
        }
        else if (line.StartsWith("end"))
        {
            if (ancestorSubgraphs.Count > 0)
                ancestorSubgraphs.RemoveAt(ancestorSubgraphs.Count - 1);
        }
        else
        {
            var nodeName = line.Split('[')[0];
            CheckIfCyclic(ancestorSubgraphs, nodeName, cyclicSubgraphs);
        }
    }

    private static void CheckIfCyclic(
        List<Subgraph> ancestorSubgraphs,
        string nodeName,
        Dictionary<int, Subgraph> cyclicSubgraphs)
    {
        var ancestor = ancestorSubgraphs.Find(s => s.Name == nodeName);
        if (ancestor is not null)
            cyclicSubgraphs[ancestor.Line] = ancestor;
    }

    public string? FixCycles(Dictionary<int, Subgraph> subgraphs)
    {
        if (subgraphs.Count == 0)
            return null;

        var lines = mermaidCode.Split('\n');

        foreach (var subgraph in subgraphs.Values)
        {
            var cyclicLine = lines[subgraph.Line];
            var parts = cyclicLine.Split("subgraph", 2);
            var beforeSubgraph = parts[0];
            var afterSubgraph = parts[1];
            var bracketParts = afterSubgraph.Split('[', 2);
            var subgraphName = bracketParts[0].Trim();

            if (subgraphName != subgraph.Name)
                return null;

            var newName = $"{subgraphName}_";
            var newLine = $"{beforeSubgraph}subgraph {newName}";
            if (bracketParts.Length > 1)
                newLine += $"[{bracketParts[1]}";

            lines[subgraph.Line] = newLine;
        }

        return string.Join('\n', lines);
    }
}
