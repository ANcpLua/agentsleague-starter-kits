using System.IO.Compression;
using System.Text.Json;

namespace ArchTools.Mermaid;

public sealed record MermaidEditorState
{
    public string Code { get; init; } = string.Empty;
    public string Mermaid { get; init; } = """{"theme": "default"}""";
    public bool UpdateDiagram { get; init; } = true;
    public bool AutoSync { get; init; } = true;
    public bool Rough { get; init; } = false;
}

public static class MermaidSerializer
{
    private const string Prefix = "pako:";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(string mermaidCode)
    {
        var state = new MermaidEditorState { Code = mermaidCode };
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(state, JsonOptions);
        var compressed = DeflateZlib(jsonBytes);
        var base64Url = ToBase64Url(compressed);
        return $"{Prefix}{base64Url}";
    }

    public static MermaidEditorState Deserialize(string serialized)
    {
        if (!serialized.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Expected '{Prefix}'-prefixed string.", nameof(serialized));

        var b64 = serialized.AsSpan(Prefix.Length);
        var data = FromBase64Url(b64);
        var jsonBytes = InflateZlib(data);
        return JsonSerializer.Deserialize<MermaidEditorState>(jsonBytes, JsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize MermaidEditorState.");
    }

    private static byte[] DeflateZlib(byte[] input)
    {
        using var ms = new MemoryStream();
        using (var z = new ZLibStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            z.Write(input, 0, input.Length);
        }
        return ms.ToArray();
    }

    private static byte[] InflateZlib(byte[] input)
    {
        using var ms = new MemoryStream(input);
        using var output = new MemoryStream();
        using (var z = new ZLibStream(ms, CompressionMode.Decompress, leaveOpen: true))
        {
            z.CopyTo(output);
        }
        return output.ToArray();
    }

    private static string ToBase64Url(byte[] data)
    {
        var s = Convert.ToBase64String(data);
        s = s.TrimEnd('=');
        s = s.Replace('+', '-').Replace('/', '_');
        return s;
    }

    private static byte[] FromBase64Url(ReadOnlySpan<char> s)
    {
        var str = new string(s).Replace('-', '+').Replace('_', '/');
        var mod = str.Length % 4;
        if (mod == 2) str += "==";
        else if (mod == 3) str += "=";
        else if (mod == 1) throw new FormatException("Invalid base64url length.");
        return Convert.FromBase64String(str);
    }
}
