using System.ComponentModel;
using ModelContextProtocol.Server;
using SupportMcpServer.Models;

namespace SupportMcpServer.Tools;

internal sealed class SupportTicketTools
{
    [McpServerTool]
    [Description("Creates a new support ticket. Returns formatted ticket details. This is a write operation.")]
    public static async Task<string> CreateTicket(
        [Description("Short title for the ticket")]
        string title,
        [Description("Full description of the issue")]
        string description,
        [Description("Priority: Low, Medium, High, Critical")]
        string priority,
        ITicketStore store,
        CancellationToken ct)
    {
        var parsedPriority = Enum.Parse<TicketPriority>(priority, true);
        var ticket = await store.CreateAsync(title, description, parsedPriority, ct);
        return FormatTicket(ticket);
    }

    [McpServerTool]
    [Description(
        "Updates the status, priority, or assignee of an existing support ticket. Returns updated ticket details.")]
    public static async Task<string> UpdateTicket(
        [Description("The ticket ID to update")]
        string ticketId,
        [Description("New status: Open, InProgress, Resolved, Closed")]
        string? status,
        [Description("New priority: Low, Medium, High, Critical")]
        string? priority,
        [Description("New assignee name or email")]
        string? assignee,
        ITicketStore store,
        CancellationToken ct)
    {
        var parsedStatus = status is not null ? Enum.Parse<TicketStatus>(status, true) : (TicketStatus?)null;
        var parsedPriority = priority is not null ? Enum.Parse<TicketPriority>(priority, true) : (TicketPriority?)null;

        var ticket = await store.UpdateAsync(ticketId, parsedStatus, parsedPriority, assignee, ct);
        return FormatTicket(ticket);
    }

    [McpServerTool]
    [Description("Retrieves a support ticket by ID. Returns all ticket details.")]
    public static async Task<string> GetTicket(
        [Description("The ticket ID to look up")]
        string ticketId,
        ITicketStore store,
        CancellationToken ct)
    {
        var ticket = await store.GetAsync(ticketId, ct)
                     ?? throw new InvalidOperationException($"Ticket {ticketId} not found.");
        return FormatTicket(ticket);
    }

    private static string FormatTicket(Ticket ticket)
    {
        return $"""
                ## Ticket #{ticket.Id}
                **Title:** {ticket.Title}
                **Description:** {ticket.Description}
                **Priority:** {ticket.Priority}
                **Status:** {ticket.Status}
                **Assignee:** {ticket.Assignee ?? "Unassigned"}
                **Created:** {ticket.CreatedAt:yyyy-MM-dd HH:mm}
                {(ticket.UpdatedAt.HasValue ? $"**Updated:** {ticket.UpdatedAt.Value:yyyy-MM-dd HH:mm}" : "")}
                """;
    }
}