namespace CertPrepAgents.Contracts;

public sealed record CuratorOutput
{
    public required string CertificationName { get; init; }
    public required IReadOnlyList<LearnModule> Modules { get; init; }
    public required IReadOnlyList<string> Domains { get; init; }
}

public sealed record LearnModule
{
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string Domain { get; init; }
    public required int EstimatedMinutes { get; init; }
}
