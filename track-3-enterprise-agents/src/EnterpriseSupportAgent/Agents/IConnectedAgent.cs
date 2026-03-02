using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace EnterpriseSupportAgent.Agents;

public interface IConnectedAgent
{
    string Name { get; }
    Task<AgentResponse> HandleAsync(string input, ITurnContext turnContext, ITurnState turnState, CancellationToken ct);
}

public sealed record AgentResponse(string Text, Attachment? Card);