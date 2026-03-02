using System.Text;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace EnterpriseSupportAgent.Services;

public sealed class McpObservabilityService : IAsyncDisposable
{
    private readonly Uri _endpoint;
    private readonly ILogger<McpObservabilityService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private McpClient? _client;

    public McpObservabilityService(Uri endpoint, ILogger<McpObservabilityService> logger)
    {
        _endpoint = endpoint;
        _logger = logger;
    }

    /// <summary>
    /// Returns formatted tool descriptions for LLM context, or null if MCP is unavailable.
    /// </summary>
    public async Task<string?> GetToolDescriptionsAsync(CancellationToken ct)
    {
        var client = await GetClientAsync(ct);
        if (client is null) return null;

        var tools = await client.ListToolsAsync(cancellationToken: ct);
        if (tools.Count == 0) return null;

        var sb = new StringBuilder();
        foreach (var tool in tools)
            sb.AppendLine($"- {tool.Name}: {tool.Description}");
        return sb.ToString();
    }

    /// <summary>
    /// Calls an MCP tool and returns the text result, or null on failure.
    /// </summary>
    public async Task<string?> CallToolAsync(string toolName, Dictionary<string, object?>? arguments,
        CancellationToken ct)
    {
        var client = await GetClientAsync(ct);
        if (client is null) return null;

        var result = await client.CallToolAsync(toolName, arguments, cancellationToken: ct);
        var sb = new StringBuilder();
        foreach (var content in result.Content)
            if (content is TextContentBlock textBlock && !string.IsNullOrEmpty(textBlock.Text))
                sb.AppendLine(textBlock.Text);
        return sb.Length > 0 ? sb.ToString() : null;
    }

    private async Task<McpClient?> GetClientAsync(CancellationToken ct)
    {
        if (_client is not null) return _client;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_client is not null) return _client;

            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = _endpoint,
                Name = "qyl-observability"
            });
            _client = await McpClient.CreateAsync(transport, cancellationToken: ct);
            _logger.LogInformation("Connected to MCP observability server at {Endpoint}", _endpoint);
            return _client;
        }
        catch (HttpRequestException ex)
        {
            // Network/connection failure — server unreachable, DNS failure, TLS error
            _logger.LogWarning(ex, "Network error connecting to MCP server at {Endpoint}", _endpoint);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            // Protocol-level failure — MCP handshake rejected, incompatible server
            _logger.LogWarning(ex, "MCP protocol error connecting to {Endpoint}", _endpoint);
            return null;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
            await _client.DisposeAsync();
        _initLock.Dispose();
    }
}
