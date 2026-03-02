---
related:
  - track-3-stack.md
sources:
  - agent-academy/docs/operative/04-automate-triggers/index.md
  - agent-academy/docs/operative/03-multi-agent/index.md
---

# Proactive Triggers — Power Automate to CEA Proactive Messaging

Deep-dive on the event trigger → CEA → proactive message pattern. Read [`track-3-stack.md`](./track-3-stack.md) first for the scoring context.

---

## The 15-Second Window Problem

Bot Framework channels enforce a 15-second response window. When Azure Bot Service delivers an activity to your CEA's `/api/messages` endpoint, your handler must return an HTTP 200 within 15 seconds or the channel closes the connection and the user sees a failure.

This is fine for interactive request/response ("What is my ticket status?" → lookup → reply). It is not fine for event-triggered async flows:

1. Power Automate fires a trigger (email received, meeting in 15 min)
2. Power Automate sends an HTTP POST to your CEA
3. Your CEA needs to: call Microsoft Graph, process data with AI, compose a response
4. That processing takes 20–60 seconds

If you block the initial HTTP handler waiting for processing to complete, you timeout. If you return immediately without storing state, you lose the ability to send a proactive reply.

The continuation token pattern solves this.

---

## The Continuation Token Solution

"Continuation token" is the common name for the pattern. In Bot Framework, the actual mechanism is `ConversationReference` storage + `BotAdapter.ContinueConversationAsync`.

### How it works

```
Power Automate fires
    → HTTP POST to /api/messages (activity delivered)
    → CEA handler starts
    → CEA stores ConversationReference (conversation address)
    → CEA responds HTTP 200 immediately with "I'm on it..."
    → Handler returns (within 15 seconds)
    → Background Task starts
    → [async work: Graph API, AI processing, etc.]
    → Background Task completes
    → CEA calls adapter.ContinueConversationAsync(reference, async (ctx, ct) => {
           await ctx.SendActivityAsync(proactiveCard);
       })
    → User receives proactive Adaptive Card
```

The `ConversationReference` is the key. It contains everything Bot Framework needs to route a message back to the right conversation without the user sending a new message:

```csharp
public class ConversationReference
{
    public string ActivityId { get; set; }       // original activity ID
    public ChannelAccount User { get; set; }     // who to address
    public ChannelAccount Bot { get; set; }      // which bot identity
    public ConversationAccount Conversation { get; set; } // channel + conversation ID
    public string ChannelId { get; set; }        // "msteams", "directline", etc.
    public string ServiceUrl { get; set; }       // Azure Bot Service endpoint to call back
}
```

Store this. Everything else is re-derivable. Without the `ServiceUrl`, you cannot send a proactive message.

---

## Code Skeleton: ASP.NET Continuation Token Handler

```csharp
// CEA activity handler — inherits from ActivityHandler (Microsoft.Agents.Builder)
public class SupportAgentHandler : ActivityHandler
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IConversationReferenceStore _store;
    private readonly IProactiveWorkQueue _workQueue;
    private readonly string _botAppId;

    public SupportAgentHandler(
        IBotFrameworkHttpAdapter adapter,
        IConversationReferenceStore store,
        IProactiveWorkQueue workQueue,
        IConfiguration config)
    {
        _adapter = adapter;
        _store = store;
        _workQueue = workQueue;
        _botAppId = config["MicrosoftAppId"];
    }

    protected override async Task OnMessageActivityAsync(
        ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        // Check if this is an event-triggered payload (not a user message)
        var channelData = turnContext.Activity.GetChannelData<EventChannelData>();

        if (channelData?.EventType == "calendarReminder")
        {
            await HandleCalendarReminderAsync(turnContext, channelData, cancellationToken);
            return;
        }

        // Normal interactive message handling
        await HandleInteractiveMessageAsync(turnContext, cancellationToken);
    }

    private async Task HandleCalendarReminderAsync(
        ITurnContext<IMessageActivity> turnContext,
        EventChannelData eventData,
        CancellationToken cancellationToken)
    {
        // Step 1: Capture the conversation reference BEFORE doing anything async
        var reference = turnContext.Activity.GetConversationReference();

        // Step 2: Persist it — use distributed cache or Cosmos DB in production
        await _store.SaveAsync(eventData.MeetingId, reference, cancellationToken);

        // Step 3: Acknowledge immediately — this response arrives within 15 seconds
        await turnContext.SendActivityAsync(
            MessageFactory.Text("Preparing your meeting briefing..."),
            cancellationToken);

        // Step 4: Queue the real work — this runs after the handler returns
        _workQueue.Enqueue(new MeetingBriefingJob
        {
            MeetingId = eventData.MeetingId,
            ReferenceKey = eventData.MeetingId,
            RequestedAt = DateTimeOffset.UtcNow
        });

        // Handler returns here — HTTP 200 sent, window closed
    }
}
```

```csharp
// Background worker — runs outside the 15-second window
public class MeetingBriefingWorker : BackgroundService
{
    private readonly IProactiveWorkQueue _queue;
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IConversationReferenceStore _store;
    private readonly IMeetingBriefingService _briefingService;
    private readonly string _botAppId;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.DequeueAsync(stoppingToken))
        {
            try
            {
                // Retrieve stored reference
                var reference = await _store.GetAsync(job.ReferenceKey, stoppingToken);
                if (reference == null) continue;

                // Do the expensive async work — no time limit here
                var briefing = await _briefingService.BuildMeetingBriefingAsync(
                    job.MeetingId, stoppingToken);

                // Send proactive message using the stored reference
                await _adapter.ContinueConversationAsync(
                    _botAppId,
                    reference,
                    async (ctx, ct) =>
                    {
                        var card = BuildBriefingCard(briefing);
                        await ctx.SendActivityAsync(
                            MessageFactory.Attachment(card.ToAttachment()), ct);
                    },
                    stoppingToken);

                // Clean up stored reference
                await _store.DeleteAsync(job.ReferenceKey, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log but do not rethrow — the background worker must keep running
                _logger.LogError(ex, "Failed to send proactive briefing for {MeetingId}", job.MeetingId);
            }
        }
    }

    private AdaptiveCard BuildBriefingCard(MeetingBriefing briefing)
    {
        return new AdaptiveCard("1.5")
        {
            Body =
            [
                new AdaptiveTextBlock { Text = briefing.Title, Weight = AdaptiveTextWeight.Bolder, Size = AdaptiveTextSize.Large },
                new AdaptiveTextBlock { Text = $"In 15 minutes — {briefing.StartTime:HH:mm}", Spacing = AdaptiveSpacing.Small },
                new AdaptiveFactSet
                {
                    Facts = briefing.Attendees.Select(a =>
                        new AdaptiveFact { Title = a.Role, Value = a.Name }).ToList()
                },
                new AdaptiveTextBlock { Text = "Agenda", Weight = AdaptiveTextWeight.Bolder, Spacing = AdaptiveSpacing.Medium },
                new AdaptiveTextBlock { Text = briefing.AgendaSummary, Wrap = true },
                new AdaptiveTextBlock { Text = "Suggested talking points", Weight = AdaptiveTextWeight.Bolder, Spacing = AdaptiveSpacing.Medium },
                new AdaptiveTextBlock { Text = briefing.TalkingPoints, Wrap = true }
            ],
            Actions =
            [
                new AdaptiveOpenUrlAction { Title = "Open meeting", Url = new Uri(briefing.JoinUrl) },
                new AdaptiveSubmitAction
                {
                    Title = "Dismiss",
                    Data = new { verb = "dismissBriefing", meetingId = briefing.MeetingId }
                }
            ]
        };
    }
}
```

```csharp
// DI registration in Program.cs
builder.Services.AddSingleton<IConversationReferenceStore, DistributedCacheReferenceStore>();
builder.Services.AddSingleton<IProactiveWorkQueue, ChannelWorkQueue>();
builder.Services.AddHostedService<MeetingBriefingWorker>();
builder.Services.AddScoped<IMeetingBriefingService, MeetingBriefingService>();
```

---

## Event Types: Three Patterns That Demo Well

### Pattern 1: Calendar — Meeting in 15 Minutes

**Power Automate trigger:** "When an upcoming event is starting soon" (Outlook Calendar connector)

Configure the trigger:
- "How soon is it starting?" → 15 minutes
- "What calendar?" → the user's primary calendar (agent-author auth)

Power Automate flow structure:
```
[Trigger: When event starts in 15 min]
    → [Get event details] (Outlook connector)
    → [Get attendee profiles] (Graph HTTP action, batch request)
    → [HTTP: POST to CEA webhook]
        Body: {
            "type": "message",
            "channelData": {
                "eventType": "calendarReminder",
                "meetingId": "@{triggerBody()?['id']}",
                "subject": "@{triggerBody()?['subject']}",
                "joinUrl": "@{triggerBody()?['onlineMeeting']?['joinUrl']}",
                "attendees": "@{triggerBody()?['attendees']}"
            }
        }
```

The CEA receives this, stores the conversation reference (which was set up when the user first installed the agent), and queues the briefing job.

**Prerequisite:** The user must have had at least one prior conversation with the agent in Teams for a `ConversationReference` to exist. The proactive trigger cannot create a conversation from scratch — it can only continue an existing one. On first install, have the agent send a welcome card and store that reference. Use that reference for all subsequent proactive messages.

### Pattern 2: Inbox — Email from Specific Person or Domain

**Power Automate trigger:** "When a new email arrives (V3)" (Outlook connector)

Configure:
- From: specific email or `*@important-customer.com`
- Folder: Inbox
- Has attachment: no (optional — filter for emails with attachments to process docs)
- Subject filter: optional keyword

Flow structure:
```
[Trigger: New email from customer domain]
    → [Condition: Is it from the priority sender list?]
        Yes:
            → [HTML to text: convert body]
            → [HTTP: POST to CEA]
                Body: {
                    "channelData": {
                        "eventType": "priorityEmail",
                        "from": "@{triggerBody()?['from']}",
                        "subject": "@{triggerBody()?['subject']}",
                        "body": "@{outputs('Html_to_text')['body/text']}"
                    }
                }
        No:
            → [Terminate]
```

The CEA processes the email body with AI, extracts action items and sentiment, and sends a proactive Adaptive Card summarizing the email with suggested actions (Reply, Create Ticket, Schedule Call).

### Pattern 3: Teams — Unread Mention in a Channel

**Power Automate trigger:** "When a message is posted in a channel" (Teams connector)

Configure:
- Team: the monitored team
- Channel: the support or escalation channel

Flow structure:
```
[Trigger: Message posted in #support channel]
    → [Condition: Does message contain @SupportAgent mention?]
        Yes:
            → [Get message details]
            → [HTTP: POST to CEA]
                Body: {
                    "channelData": {
                        "eventType": "teamsMention",
                        "messageId": "@{triggerBody()?['messageId']}",
                        "text": "@{triggerBody()?['body']?['content']}",
                        "from": "@{triggerBody()?['from']?['user']?['displayName']}"
                    }
                }
```

The CEA classifies the request (bug report, feature request, access issue), routes to the appropriate connected agent, and replies in-channel with an Adaptive Card.

---

## Wiring Power Automate HTTP Trigger to Azure Bot Service

The HTTP action in Power Automate must authenticate as your bot. The bot validates incoming activities using the Bot Framework authentication middleware — requests without valid bot credentials are rejected.

### Getting the bot token in Power Automate

Add an HTTP action before your CEA POST to fetch a bot access token:

```json
{
  "method": "POST",
  "uri": "https://login.microsoftonline.com/@{parameters('TenantId')}/oauth2/v2.0/token",
  "headers": { "Content-Type": "application/x-www-form-urlencoded" },
  "body": "grant_type=client_credentials&client_id=@{parameters('BotAppId')}&client_secret=@{parameters('BotAppSecret')}&scope=https://api.botframework.com/.default"
}
```

Store `BotAppId` and `BotAppSecret` in Power Automate environment variables — not in the flow body.

### The CEA /api/messages endpoint

Your CEA exposes `/api/messages` as the standard Bot Framework endpoint. In `Program.cs`:

```csharp
app.MapPost("/api/messages", async (HttpRequest req, HttpResponse res,
    IBotFrameworkHttpAdapter adapter, IBot bot) =>
{
    await adapter.ProcessAsync(req, res, bot);
});
```

The `BotFrameworkHttpAdapter` validates the incoming request's Authorization header against the bot's app ID and password. Configure it with:

```csharp
builder.Services.AddBotFrameworkAuthentication();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CloudAdapterWithErrorHandler>();
```

In `appsettings.json` (values from environment in production):
```json
{
  "MicrosoftAppType": "MultiTenant",
  "MicrosoftAppId": "<bot-app-id>",
  "MicrosoftAppPassword": "<bot-app-secret>"
}
```

### End-to-end flow verification

Test in this order:

1. Send a manual HTTP POST to `/api/messages` from Postman with a hardcoded activity — confirms your CEA receives and processes activities.
2. Trigger the Power Automate flow manually — confirms the flow can reach your CEA endpoint.
3. Trigger the flow via the actual event (send a test email, create a calendar event) — confirms the trigger fires correctly.
4. Confirm the proactive message appears in Teams — confirms the `ConversationReference` is valid and `ContinueConversationAsync` works.

Do not skip step 1. Most failures in proactive messaging come from malformed activities, not from the continuation token logic.

---

## Common Failure Modes

**"Method not allowed" on the `/mcp` endpoint**
This is normal for the MCP server — the protocol uses a POST handshake, and a GET from a browser returns this error. It means the server is running. Ignore it.

**ConversationReference is null**
The user has never started a conversation with the bot in Teams. Send a welcome message on app install (via `onConversationUpdateActivity`) and store that reference.

**ContinueConversationAsync throws 401**
Your bot app ID or password is wrong, or the token has expired. Bot Framework tokens expire — do not cache them longer than 55 minutes.

**Power Automate flow fails silently**
Add a "Send me an email" action in the failure branch of Power Automate for development. In production, use the run history in Power Automate portal to inspect the failure.

**Teams message arrives but the card doesn't render**
Validate your Adaptive Card JSON at `adaptivecards.io/designer` before sending. Teams is strict about schema version — use version `1.5` for full Teams compatibility, not `2.x`.
