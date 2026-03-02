using Microsoft.Extensions.AI;

namespace ArchTools.Llm;

public static class DiagramGenerator
{
    public static async Task<string> GenerateAsync(
        IChatClient chatClient,
        IList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        return response.Text;
    }
}
