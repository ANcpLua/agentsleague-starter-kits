using System.ComponentModel;
using System.Text.Json;
using CertPrepAgents.Contracts;

namespace CertPrepAgents.Tools;

internal static class LearnSearchTool
{
    [Description("Searches Microsoft Learn for training modules related to a certification domain. Returns module titles and URLs.")]
    public static async Task<string> SearchLearn(
        [Description("The certification name, e.g. 'AZ-900'")] string certification,
        [Description("The specific domain to search for, e.g. 'networking'")] string domain,
        HttpClient httpClient)
    {
        var query = Uri.EscapeDataString($"{certification} {domain}");
        var requestUri = $"https://learn.microsoft.com/api/search?search={query}&locale=en-us&facet=products&%24top=5";

        try
        {
            using var response = await httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var results = new List<LearnModule>();
            if (doc.RootElement.TryGetProperty("results", out var resultsArray))
            {
                foreach (var result in resultsArray.EnumerateArray())
                {
                    var title = result.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                    var url = result.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";

                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url))
                    {
                        results.Add(new LearnModule
                        {
                            Title = title,
                            Url = url.StartsWith("http") ? url : $"https://learn.microsoft.com{url}",
                            Domain = domain,
                            EstimatedMinutes = 45
                        });
                    }
                }
            }

            return JsonSerializer.Serialize(results, CertPrepJsonContext.Default.IReadOnlyListLearnModule);
        }
        catch (HttpRequestException)
        {
            IReadOnlyList<LearnModule> fallback =
            [
                new LearnModule
                {
                    Title = $"Microsoft Learn: {certification} - {domain}",
                    Url = $"https://learn.microsoft.com/en-us/search/?terms={query}",
                    Domain = domain,
                    EstimatedMinutes = 60
                }
            ];
            return JsonSerializer.Serialize(fallback, CertPrepJsonContext.Default.IReadOnlyListLearnModule);
        }
    }
}
