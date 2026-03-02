namespace EnterpriseSupportAgent.Models;

public sealed record TicketInfo
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Priority { get; init; } = "";
    public string Status { get; init; } = "";
    public string? Assignee { get; init; }
    public string CreatedAt { get; init; } = "";
    public string? UpdatedAt { get; init; }
}