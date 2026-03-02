namespace CertPrepAgents.Contracts;

public sealed record RemediationPlan
{
    public required IReadOnlyList<WeakDomain> WeakDomains { get; init; }
    public required int TotalRemediationHours { get; init; }
}

public sealed record WeakDomain
{
    public required string Domain { get; init; }
    public required double CurrentScore { get; init; }
    public required double TargetScore { get; init; }
    public required string Reason { get; init; }
    public required IReadOnlyList<string> RecommendedModuleUrls { get; init; }
    public required int EstimatedHours { get; init; }
}
