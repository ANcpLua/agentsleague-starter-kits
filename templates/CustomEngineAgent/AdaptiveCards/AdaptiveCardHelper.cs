using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Agents.Core.Models;

namespace CustomEngineAgent.AdaptiveCards;

/// <summary>
/// Helper to load Adaptive Card templates, bind data, and create Bot Framework attachments.
/// Card templates use ${expression} placeholders that get replaced with actual values.
/// </summary>
public static partial class AdaptiveCardHelper
{
    private static readonly string CardsPath = Path.Combine(AppContext.BaseDirectory, "AdaptiveCards");

    /// <summary>
    /// Creates a result card showing structured output with title, status, facts, and actions.
    /// </summary>
    public static Attachment CreateResultCard(
        string title,
        string subtitle,
        string status,
        string statusColor,
        string summary,
        IReadOnlyList<(string Key, string Value)> facts,
        string? detailUrl = null,
        string? itemId = null,
        string? iconUrl = null)
    {
        var data = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["subtitle"] = subtitle,
            ["status"] = status,
            ["statusColor"] = statusColor,
            ["summary"] = summary,
            ["detailUrl"] = detailUrl ?? "#",
            ["itemId"] = itemId ?? "",
            ["iconUrl"] = iconUrl ?? "",
            ["facts"] = facts.Select(f => new { key = f.Key, value = f.Value }).ToArray()
        };

        return BuildAttachment("ResultCard.json", data);
    }

    /// <summary>
    /// Creates an input card for collecting user preferences or form data.
    /// </summary>
    public static Attachment CreateInputCard(
        string title,
        string description,
        string field1Label,
        string field1Placeholder,
        IReadOnlyList<(string Title, string Value)> choices,
        string field2Label,
        string field2Placeholder,
        string field3Label,
        string field3Toggle,
        string? formId = null)
    {
        var data = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["description"] = description,
            ["field1Label"] = field1Label,
            ["field1Placeholder"] = field1Placeholder,
            ["choices"] = choices.Select(c => new { title = c.Title, value = c.Value }).ToArray(),
            ["field2Label"] = field2Label,
            ["field2Placeholder"] = field2Placeholder,
            ["field3Label"] = field3Label,
            ["field3Toggle"] = field3Toggle,
            ["formId"] = formId ?? Guid.NewGuid().ToString("N")
        };

        return BuildAttachment("InputCard.json", data);
    }

    /// <summary>
    /// Creates a confirmation card for approval flows (approve/reject/request info).
    /// </summary>
    public static Attachment CreateConfirmationCard(
        string title,
        string description,
        string requestedBy,
        string requestDate,
        string amount,
        string justification,
        string? requestId = null)
    {
        var data = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["description"] = description,
            ["requestedBy"] = requestedBy,
            ["requestDate"] = requestDate,
            ["amount"] = amount,
            ["justification"] = justification,
            ["requestId"] = requestId ?? Guid.NewGuid().ToString("N")
        };

        return BuildAttachment("ConfirmationCard.json", data);
    }

    private static Attachment BuildAttachment(string templateFileName, Dictionary<string, object?> data)
    {
        var templatePath = Path.Combine(CardsPath, templateFileName);
        var template = File.ReadAllText(templatePath);
        var rendered = BindTemplate(template, data);

        return new Attachment
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = JsonSerializer.Deserialize<JsonElement>(rendered)
        };
    }

    /// <summary>
    /// Simple template binding: replaces ${key} with values from the data dictionary.
    /// Supports nested access for arrays: ${facts[0].key} and ${choices[1].title}.
    /// </summary>
    private static string BindTemplate(string template, Dictionary<string, object?> data)
    {
        return TemplatePlaceholder().Replace(template, match =>
        {
            var expression = match.Groups[1].Value;

            // Handle array access: facts[0].key
            var arrayMatch = ArrayAccessPattern().Match(expression);
            if (arrayMatch.Success)
            {
                var arrayName = arrayMatch.Groups[1].Value;
                var index = int.Parse(arrayMatch.Groups[2].Value);
                var property = arrayMatch.Groups[3].Value;

                if (data.TryGetValue(arrayName, out var arrayValue) && arrayValue is object[] arr && index < arr.Length)
                {
                    var element = arr[index];
                    var prop = element.GetType().GetProperty(property);
                    return prop?.GetValue(element)?.ToString() ?? "";
                }
                return "";
            }

            // Handle simple key access
            if (data.TryGetValue(expression, out var value))
                return value?.ToString() ?? "";

            return match.Value; // Leave unresolved placeholders as-is
        });
    }

    [GeneratedRegex(@"\$\{(\w+)\[(\d+)\]\.(\w+)\}")]
    private static partial Regex ArrayAccessPattern();

    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex TemplatePlaceholder();
}
