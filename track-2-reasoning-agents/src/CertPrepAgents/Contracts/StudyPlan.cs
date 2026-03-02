namespace CertPrepAgents.Contracts;

public sealed record StudyPlan
{
    public required string CertificationName { get; init; }
    public required IReadOnlyList<StudyDomain> Domains { get; init; }
    public required int TotalEstimatedHours { get; init; }
}

public sealed record StudyDomain
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> ModuleUrls { get; init; }
    public required int EstimatedHours { get; init; }
}
