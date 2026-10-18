using System.Globalization;
using System.Text.RegularExpressions;
using FahadHassan.PulseSend.Models;

namespace FahadHassan.PulseSend.Services;

public sealed class MessageTemplateRenderer : IMessageTemplateRenderer
{
    private static readonly Regex PlaceholderRegex = new(@"\{(?<key>[A-Za-z]+)\}", RegexOptions.Compiled);

    public string Render(string template, Customer customer)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(customer);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Creditnumber"] = customer.CreditNumber,
            ["Name"] = customer.Name,
            ["amount"] = customer.Amount.ToString("0.00", CultureInfo.GetCultureInfo("de-DE")),
            ["dueDate"] = customer.DueDate.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"))
        };

        var rendered = PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            if (!values.TryGetValue(key, out var value))
                throw new InvalidOperationException($"Unsupported template placeholder '{{{key}}}'.");
            return value;
        });

        if (rendered.Contains('{') || rendered.Contains('}'))
            throw new InvalidOperationException("Template contains malformed or unsupported placeholders.");

        return rendered.Replace("\\n", "\n", StringComparison.Ordinal);
    }
}
