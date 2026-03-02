namespace CertPrepAgents.Contracts;

public enum Decision { Pass, Remediate }

public sealed record PlannerDecision
{
    public required Decision Decision { get; init; }
    public required string Reasoning { get; init; }
    public RemediationPlan? RemediationPlan { get; init; }
    public string? RecommendedCertification { get; init; }
}
