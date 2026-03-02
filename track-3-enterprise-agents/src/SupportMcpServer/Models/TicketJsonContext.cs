using System.Text.Json.Serialization;

namespace SupportMcpServer.Models;

[JsonSerializable(typeof(Ticket))]
[JsonSerializable(typeof(List<Ticket>))]
internal sealed partial class TicketJsonContext : JsonSerializerContext;