using System.Text;
using System.Text.RegularExpressions;
using EnterpriseSupportAgent.AdaptiveCards;
using EnterpriseSupportAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using OpenAI.Chat;

namespace EnterpriseSupportAgent.Agents;

public sealed partial class TicketAgent : IConnectedAgent
{
    private readonly IChatClientService _chatClient;

    public TicketAgent(IChatClientService chatClient)
    {
        _chatClient = chatClient;
    }

    public string Name => "TicketAgent";

    public async Task<AgentResponse> HandleAsync(string input, ITurnContext turnContext, ITurnState turnState,
        CancellationToken ct)
    {
        // Classify the sub-intent for ticket operations
        var subIntent = await ClassifySubIntentAsync(input, ct);

        return subIntent switch
        {
            "create" => await HandleCreateAsync(input, ct),
            "update" => await HandleUpdateAsync(input, ct),
            "get" => await HandleGetAsync(input),
            "list" => HandleList(),
            _ => await HandleCreateAsync(input, ct) // default to create for ticket intent
        };
    }

    private async Task<string> ClassifySubIntentAsync(string input, CancellationToken ct)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                                  Classify the ticket sub-intent into exactly one category:
                                  - create: user wants to create a new ticket
                                  - update: user wants to update, escalate, close, or reassign an existing ticket
                                  - get: user wants to check status or view a specific ticket
                                  - list: user wants to see all tickets or recent tickets
                                  Return ONLY the category name.
                                  """),
            new UserChatMessage(input)
        };

        var response = _chatClient.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);

        return result.ToString().Trim().ToLowerInvariant();
    }

    private async Task<AgentResponse> HandleCreateAsync(string input, CancellationToken ct)
    {
        // Use AI to extract ticket details from the user's message
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                                  Extract ticket details from the user message. Respond in exactly this format:
                                  TITLE: <short title>
                                  DESCRIPTION: <full description>
                                  PRIORITY: <Low|Medium|High|Critical>
                                  If any field is ambiguous, make a reasonable inference.
                                  """),
            new UserChatMessage(input)
        };

        var response = _chatClient.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);

        var parsed = result.ToString();
        var title = ExtractField(parsed, "TITLE") ?? "Support Request";
        var description = ExtractField(parsed, "DESCRIPTION") ?? input;
        var priority = ExtractField(parsed, "PRIORITY") ?? "Medium";

        // Generate a ticket ID (simulating MCP server call)
        var ticketId = Guid.NewGuid().ToString("N")[..8];
        var createdAt = TimeProvider.System.GetUtcNow();

        var card = AdaptiveCardHelper.CreateResultCard(
            $"Ticket #{ticketId}",
            title,
            "Open",
            priority is "Critical" or "High" ? "Attention" : "Good",
            description,
            [
                ("Priority", priority),
                ("Created", createdAt.ToString("g")),
                ("Assigned", "Unassigned")
            ],
            itemId: ticketId);

        return new AgentResponse(
            $"Ticket #{ticketId} created: {title} [{priority}]",
            card);
    }

    private async Task<AgentResponse> HandleUpdateAsync(string input, CancellationToken ct)
    {
        // Use AI to extract update details
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                                  Extract ticket update details from the user message. Respond in exactly this format:
                                  TICKET_ID: <id or "unknown">
                                  STATUS: <Open|InProgress|Resolved|Closed or "unchanged">
                                  PRIORITY: <Low|Medium|High|Critical or "unchanged">
                                  ASSIGNEE: <name or "unchanged">
                                  """),
            new UserChatMessage(input)
        };

        var response = _chatClient.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);

        var parsed = result.ToString();
        var ticketId = ExtractField(parsed, "TICKET_ID") ?? "unknown";
        var status = ExtractField(parsed, "STATUS") ?? "InProgress";
        var priority = ExtractField(parsed, "PRIORITY") ?? "unchanged";
        var assignee = ExtractField(parsed, "ASSIGNEE") ?? "unchanged";

        if (ticketId == "unknown")
            ticketId = Guid.NewGuid().ToString("N")[..8];

        var displayStatus = status != "unchanged" ? status : "InProgress";
        var displayPriority = priority != "unchanged" ? priority : "Medium";

        var card = AdaptiveCardHelper.CreateResultCard(
            $"Ticket #{ticketId} Updated",
            "Ticket has been updated",
            displayStatus,
            displayStatus == "Resolved" ? "Good" : "Warning",
            $"Updates applied based on your request: {input}",
            [
                ("Status", displayStatus),
                ("Priority", displayPriority),
                ("Assignee", assignee != "unchanged" ? assignee : "Unassigned")
            ],
            itemId: ticketId);

        return new AgentResponse(
            $"Ticket #{ticketId} updated — Status: {displayStatus}",
            card);
    }

    private static Task<AgentResponse> HandleGetAsync(string input)
    {
        try
        {
            // Extract ticket ID from user message
            var idMatch = TicketIdPattern().Match(input);
            var ticketId = idMatch.Success ? idMatch.Groups[1].Value : Guid.NewGuid().ToString("N")[..8];

            var card = AdaptiveCardHelper.CreateResultCard(
                $"Ticket #{ticketId}",
                "Ticket Details",
                "Open",
                "Good",
                $"Details for ticket #{ticketId} as requested.",
                [
                    ("Status", "Open"),
                    ("Priority", "Medium"),
                    ("Assigned", "IT Support Team")
                ],
                itemId: ticketId);

            return Task.FromResult(new AgentResponse(
                $"Retrieved ticket #{ticketId}",
                card));
        }
        catch (Exception exception)
        {
            return Task.FromException<AgentResponse>(exception);
        }
    }

    private static AgentResponse HandleList()
    {
        var card = AdaptiveCardHelper.CreateResultCard(
            "Active Tickets",
            "Your open support tickets",
            "3 Open",
            "Warning",
            "Showing your most recent open tickets. Use 'get ticket #<id>' for details.",
            [
                ("Open", "3"),
                ("In Progress", "2"),
                ("Resolved Today", "1")
            ]);

        return new AgentResponse(
            "Here are your active tickets.",
            card);
    }

    private static string? ExtractField(string text, string fieldName)
    {
        var pattern = $@"{fieldName}:\s*(.+?)(?:\n|$)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    [GeneratedRegex(@"#?([a-f0-9]{6,8})", RegexOptions.IgnoreCase)]
    private static partial Regex TicketIdPattern();
}