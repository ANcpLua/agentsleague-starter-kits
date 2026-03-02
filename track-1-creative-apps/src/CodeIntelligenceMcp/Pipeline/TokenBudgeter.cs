namespace ArchTools.Pipeline;

public static class TokenBudgeter
{
    public static int EstimateTokens(string text) => text.Length / 4;

    public static List<SourceFile> SelectFiles(IReadOnlyList<SourceFile> files, int maxChars)
    {
        var selected = new List<SourceFile>();
        var totalChars = 0;

        foreach (var file in files)
        {
            var fileChars = PromptBuilder.EncodeFile(file).Length;
            if (totalChars + fileChars > maxChars)
                continue;

            totalChars += fileChars;
            selected.Add(file);
        }

        return selected;
    }
}
