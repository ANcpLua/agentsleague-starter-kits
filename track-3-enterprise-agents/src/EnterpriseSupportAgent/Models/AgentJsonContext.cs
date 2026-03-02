using System.Text.Json.Serialization;

namespace EnterpriseSupportAgent.Models;

[JsonSerializable(typeof(KnowledgeResult))]
[JsonSerializable(typeof(Citation))]
[JsonSerializable(typeof(List<Citation>))]
[JsonSerializable(typeof(TicketInfo))]
[JsonSerializable(typeof(List<TicketInfo>))]
internal sealed partial class AgentJsonContext : JsonSerializerContext;