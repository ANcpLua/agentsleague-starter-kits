using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace Qyl.Samples.MafAgent;

/// <summary>
///     Mock chat client that simulates LLM responses without needing Azure credentials.
///     Produces realistic token counts and latency for observability demonstration.
/// </summary>
internal sealed class MockChatClient : IChatClient
{
    private static readonly string[] s_responses =
    [
        "I've analyzed the system metrics. CPU usage is at 78% across the cluster, with node-3 showing memory pressure at 92%. The latency spike at 14:32 UTC correlates with a deployment rollout that triggered pod restarts.",
        "Based on the trace data, the root cause is a connection pool exhaustion in the database layer. The pg_stat_activity shows 47 idle-in-transaction connections from the order-service. Recommend increasing pool_size from 20 to 50 and adding a connection timeout of 30s.",
        "The error rate increased from 0.1% to 3.2% after the v2.4.1 release. Stack traces show a NullReferenceException in OrderProcessor.ValidateShipping() when the address field is null. This is a regression from commit abc123 that removed the null check during the refactor."
    ];

    private int _responseIndex;

    public ChatClientMetadata Metadata { get; } = new("mock-provider", null, "mock-gpt-4o");

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Simulate LLM latency
        await Task.Delay(Random.Shared.Next(100, 250), cancellationToken).ConfigureAwait(false);

        var messageList = messages.ToList();
        var inputChars = messageList.Sum(m => m.Text?.Length ?? 0);

        var responseText = s_responses[Interlocked.Increment(ref _responseIndex) % s_responses.Length];

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText))
        {
            ModelId = "mock-gpt-4o",
            Usage = new UsageDetails
            {
                InputTokenCount = inputChars / 4,  // ~4 chars per token
                OutputTokenCount = responseText.Length / 4,
                TotalTokenCount = (inputChars + responseText.Length) / 4
            }
        };
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        yield return new ChatResponseUpdate(ChatRole.Assistant, response.Text)
        {
            ModelId = response.ModelId
        };
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType == typeof(MockChatClient) ? this : null;

    public void Dispose() { }
}
