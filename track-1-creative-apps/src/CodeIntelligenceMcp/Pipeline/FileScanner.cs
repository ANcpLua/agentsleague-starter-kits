using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace ArchTools.Pipeline;

public sealed class FileScanner(
    string basePath,
    IReadOnlyList<string> extensions,
    IReadOnlyList<string> excludePatterns,
    int maxFiles)
{
    public List<SourceFile> Scan()
    {
        var matcher = new Matcher();

        foreach (var ext in extensions)
            matcher.AddInclude($"**/*.{ext}");

        foreach (var pattern in excludePatterns)
            matcher.AddExclude(pattern);

        var directoryInfo = new DirectoryInfoWrapper(new DirectoryInfo(basePath));
        var result = matcher.Execute(directoryInfo);

        return result.Files
            .Take(maxFiles)
            .Select(match =>
            {
                var fullPath = Path.Combine(basePath, match.Path);
                var content = File.ReadAllText(fullPath);
                var extension = Path.GetExtension(fullPath).TrimStart('.');
                return new SourceFile(fullPath, content, extension);
            })
            .ToList();
    }
}
