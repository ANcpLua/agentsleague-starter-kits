namespace CertPrepAgents.Contracts;

public sealed record AssessmentResult
{
    public required bool Passed { get; init; }
    public required double OverallScore { get; init; }
    public required IReadOnlyList<DomainScore> DomainScores { get; init; }
    public required IReadOnlyList<QuestionResult> Questions { get; init; }
}

public sealed record DomainScore
{
    public required string Domain { get; init; }
    public required double Score { get; init; }
    public required int Correct { get; init; }
    public required int Total { get; init; }
}

public sealed record QuestionResult
{
    public required string Domain { get; init; }
    public required string Question { get; init; }
    public required string UserAnswer { get; init; }
    public required string CorrectAnswer { get; init; }
    public required bool IsCorrect { get; init; }
    public required string Reasoning { get; init; }
}
