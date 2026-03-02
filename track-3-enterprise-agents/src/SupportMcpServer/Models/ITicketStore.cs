namespace SupportMcpServer.Models;

internal interface ITicketStore
{
    Task<Ticket> CreateAsync(string title, string description, TicketPriority priority, CancellationToken ct);
    Task<Ticket?> GetAsync(string id, CancellationToken ct);

    Task<Ticket> UpdateAsync(string id, TicketStatus? status, TicketPriority? priority, string? assignee,
        CancellationToken ct);

    Task<IReadOnlyList<Ticket>> ListAsync(TicketStatus? status, int limit, CancellationToken ct);
}