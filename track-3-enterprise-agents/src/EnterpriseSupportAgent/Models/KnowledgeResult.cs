namespace EnterpriseSupportAgent.Models;

public sealed record KnowledgeResult
{
    public required string Answer { get; init; }
    public required IReadOnlyList<Citation> Citations { get; init; }
}

public sealed record Citation
{
    public required string Source { get; init; }
    public required string Excerpt { get; init; }
}