using System.Text;
using System.Text.RegularExpressions;
using EnterpriseSupportAgent.AdaptiveCards;
using EnterpriseSupportAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using OpenAI.Chat;

namespace EnterpriseSupportAgent.Agents;

public sealed class NotifyAgent(IChatClientService chatClient) : IConnectedAgent
{
    public string Name => "NotifyAgent";

    public async Task<AgentResponse> HandleAsync(string input, ITurnContext turnContext, ITurnState turnState,
        CancellationToken ct)
    {
        // Use AI to extract notification details
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                                  Extract notification details from the user message. Respond in exactly this format:
                                  TITLE: <notification title>
                                  DESCRIPTION: <what the notification is about>
                                  RECIPIENTS: <who should be notified, or "IT Team" if unclear>
                                  URGENCY: <Low|Medium|High>
                                  """),
            new UserChatMessage(input)
        };

        var response = chatClient.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);

        var parsed = result.ToString();
        var title = ExtractField(parsed, "TITLE") ?? "Notification";
        var description = ExtractField(parsed, "DESCRIPTION") ?? input;
        var recipients = ExtractField(parsed, "RECIPIENTS") ?? "IT Team";
        var urgency = ExtractField(parsed, "URGENCY") ?? "Medium";

        var requestId = Guid.NewGuid().ToString("N")[..8];
        var now = TimeProvider.System.GetUtcNow();

        // Use ConfirmationCard for notification approval flow
        var card = AdaptiveCardHelper.CreateConfirmationCard(
            $"Send Notification: {title}",
            description,
            "You",
            now.ToString("g"),
            $"Urgency: {urgency}",
            $"Recipients: {recipients}",
            requestId);

        return new AgentResponse(
            $"Notification prepared for {recipients}: {title}. Please confirm to send.",
            card);
    }

    private static string? ExtractField(string text, string fieldName)
    {
        var pattern = $@"{fieldName}:\s*(.+?)(?:\n|$)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}