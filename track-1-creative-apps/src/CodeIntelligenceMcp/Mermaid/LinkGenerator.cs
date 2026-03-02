namespace ArchTools.Mermaid;

public sealed class MermaidLinkGenerator
{
    private const string BaseUrl = "https://mermaid.live/";
    private readonly string _serializedDiagram;

    public MermaidLinkGenerator(string mermaidCode)
    {
        _serializedDiagram = MermaidSerializer.Serialize(mermaidCode);
    }

    public string CreateEditLink() => $"{BaseUrl}edit#{_serializedDiagram}";
    public string CreateViewLink() => $"{BaseUrl}view#{_serializedDiagram}";
}
