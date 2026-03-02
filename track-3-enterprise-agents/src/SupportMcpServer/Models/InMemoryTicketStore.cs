using System.Collections.Concurrent;

namespace SupportMcpServer.Models;

internal sealed class InMemoryTicketStore : ITicketStore
{
    private readonly ConcurrentDictionary<string, Ticket> _tickets = new();

    public Task<Ticket> CreateAsync(string title, string description, TicketPriority priority, CancellationToken ct)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = title,
            Description = description,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = TimeProvider.System.GetUtcNow()
        };

        _tickets[ticket.Id] = ticket;
        return Task.FromResult(ticket);
    }

    public Task<Ticket?> GetAsync(string id, CancellationToken ct)
    {
        _tickets.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }

    public Task<Ticket> UpdateAsync(string id, TicketStatus? status, TicketPriority? priority, string? assignee,
        CancellationToken ct)
    {
        if (!_tickets.TryGetValue(id, out var existing))
            throw new InvalidOperationException($"Ticket {id} not found.");

        var updated = existing with
        {
            Status = status ?? existing.Status,
            Priority = priority ?? existing.Priority,
            Assignee = assignee ?? existing.Assignee,
            UpdatedAt = TimeProvider.System.GetUtcNow()
        };

        _tickets[id] = updated;
        return Task.FromResult(updated);
    }

    public Task<IReadOnlyList<Ticket>> ListAsync(TicketStatus? status, int limit, CancellationToken ct)
    {
        var query = _tickets.Values.AsEnumerable();
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        IReadOnlyList<Ticket> result = query
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }
}