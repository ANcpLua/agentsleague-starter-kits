using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnterpriseSupportAgent.Agents;
using EnterpriseSupportAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using OpenAI.Chat;

namespace EnterpriseSupportAgent;

public class Bot : AgentApplication, IDisposable
{
    private readonly IChatClientService _chatClientService;
    private readonly HttpClient _httpClient = new();
    private readonly IConnectedAgent _knowledgeAgent;
    private readonly IConnectedAgent _notifyAgent;
    private readonly IConnectedAgent _ticketAgent;

    public Bot(
        AgentApplicationOptions options,
        IChatClientService chatClientService,
        TicketAgent ticketAgent,
        KnowledgeAgent knowledgeAgent,
        NotifyAgent notifyAgent) : base(options)
    {
        _chatClientService = chatClientService;
        _ticketAgent = ticketAgent;
        _knowledgeAgent = knowledgeAgent;
        _notifyAgent = notifyAgent;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    [Route(RouteType = RouteType.Activity, Type = ActivityTypes.Message, Rank = RouteRank.Last, SignInHandlers = "me")]
    protected async Task OnMessageAsync(ITurnContext turnContext, ITurnState turnState,
        CancellationToken cancellationToken)
    {
        try
        {
            await turnContext.StreamingResponse.QueueInformativeUpdateAsync("Working on it...", cancellationToken);

            // Fetch/cache user profile
            var userProfile = turnState.Conversation.GetCachedUserProfile();
            if (userProfile == null)
            {
                var accessToken = await UserAuthorization.GetTurnTokenAsync(turnContext,
                    UserAuthorization.DefaultHandlerName, cancellationToken);
                userProfile = await GetUserProfileAsync(accessToken, cancellationToken);
                turnState.Conversation.SetCachedUserProfile(userProfile);
            }

            var currentUserMessage = turnContext.Activity.Text;
            turnState.Conversation.AddMessageToHistory("user", currentUserMessage);

            // Classify intent and route to connected agent
            var intent = await ClassifyIntentAsync(currentUserMessage, cancellationToken);
            var agent = intent switch
            {
                "ticket" => _ticketAgent,
                "notify" => _notifyAgent,
                _ => _knowledgeAgent // default fallback
            };

            var result = await agent.HandleAsync(currentUserMessage, turnContext, turnState, cancellationToken);

            // Store response in conversation history
            turnState.Conversation.AddMessageToHistory("assistant", result.Text);

            // Send the Adaptive Card if available, otherwise stream text
            if (result.Card is not null)
            {
                turnContext.StreamingResponse.QueueTextChunk(result.Text);
                await turnContext.StreamingResponse.EndStreamAsync(cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(result.Card), cancellationToken);
            }
            else
            {
                turnContext.StreamingResponse.QueueTextChunk(result.Text);
                await turnContext.StreamingResponse.EndStreamAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            turnContext.StreamingResponse.QueueTextChunk($"I encountered an error: {ex.Message}");
            await turnContext.StreamingResponse.EndStreamAsync(cancellationToken);
        }
    }

    [Route(RouteType = RouteType.Activity, Type = ActivityTypes.Invoke)]
    protected async Task OnInvokeAsync(ITurnContext turnContext, ITurnState turnState,
        CancellationToken cancellationToken)
    {
        var activity = turnContext.Activity;
        if (activity.Value is not JsonElement valueElement)
            return;

        string? verb = null;
        string? ticketId = null;

        if (valueElement.TryGetProperty("action", out var actionElement))
        {
            if (actionElement.TryGetProperty("verb", out var verbElement))
                verb = verbElement.GetString();
            if (actionElement.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("ticketId", out var tidElement))
                ticketId = tidElement.GetString();
        }

        // Also check top-level for Action.Submit style data
        if (verb is null && valueElement.TryGetProperty("action", out var topAction) &&
            topAction.ValueKind == JsonValueKind.String)
            verb = topAction.GetString();
        if (ticketId is null && valueElement.TryGetProperty("ticketId", out var topTid))
            ticketId = topTid.GetString();

        var responseText = verb switch
        {
            "escalateTicket" => $"Ticket #{ticketId ?? "unknown"} has been escalated to the IT Engineering team.",
            "closeTicket" => $"Ticket #{ticketId ?? "unknown"} has been closed.",
            "viewTicket" => $"Viewing details for ticket #{ticketId ?? "unknown"}.",
            "approveEscalation" => $"Escalation for ticket #{ticketId ?? "unknown"} approved.",
            "rejectEscalation" => $"Escalation for ticket #{ticketId ?? "unknown"} rejected.",
            "approve" =>
                $"Request #{(valueElement.TryGetProperty("requestId", out var rid) ? rid.GetString() : "unknown")} approved.",
            "reject" => "Request rejected.",
            _ => "Action received."
        };

        await turnContext.SendActivityAsync(responseText, cancellationToken: cancellationToken);
    }

    [Route(RouteType = RouteType.Message, Type = ActivityTypes.Message, Text = "-reset")]
    protected async Task Reset(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        await UserAuthorization.SignOutUserAsync(turnContext, turnState, "me", cancellationToken);
        turnState.Conversation.ClearConversationHistory();
        turnState.Conversation.ClearCachedUserProfile();
        await turnContext.SendActivityAsync("Reset complete", cancellationToken: cancellationToken);
    }

    private async Task<string> ClassifyIntentAsync(string input, CancellationToken ct)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                                  Classify the user intent into exactly one category:
                                  - ticket: user wants to create, update, check status, or manage a support ticket
                                  - knowledge: user is asking a question about products, policies, IT procedures, or needs information
                                  - notify: user wants to send a notification, alert, or message to a team
                                  Return ONLY the category name, nothing else.
                                  """),
            new UserChatMessage(input)
        };

        var response = _chatClientService.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);

        return result.ToString().Trim().ToLowerInvariant();
    }

    private async Task<UserProfile> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(
            "https://graph.microsoft.com/v1.0/me?$select=department,jobTitle,preferredLanguage,displayName,givenName,companyName,userPrincipalName,id",
            cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<UserProfile>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}