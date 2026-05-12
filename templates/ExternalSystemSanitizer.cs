using System.Globalization;
using System.Text;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — keep low-level canonicalization separate so translators stay focused on meaning, not string surgery.
public sealed class ExternalSystemSanitizer
{
    public string? NormalizeFreeText(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        var normalized = rawValue.Normalize(NormalizationForm.FormKC).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    public string NormalizeToken(string? rawValue)
    {
        var normalized = NormalizeFreeText(rawValue);
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        return new string(normalized
            .Where(ch => !char.IsWhiteSpace(ch) && ch != '-' && ch != '_' && ch != ':')
            .ToArray())
            .ToLowerInvariant();
    }

    public DateOnly? ParseExternalDate(string? rawValue)
    {
        var digits = new string((rawValue ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return null;

        if (digits.Length == 7)
        {
            var rocYear = int.Parse(digits[..3], CultureInfo.InvariantCulture);
            var month = int.Parse(digits.Substring(3, 2), CultureInfo.InvariantCulture);
            var day = int.Parse(digits.Substring(5, 2), CultureInfo.InvariantCulture);
            return new DateOnly(rocYear + 1911, month, day);
        }

        if (digits.Length == 8 &&
            DateOnly.TryParseExact(digits, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new ExternalSystemProtocolException($"Could not parse external date value '{rawValue}'.");
    }
}
