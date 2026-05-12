using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — dispute/evidence capture should be separate from normal app logs when the integration is high-risk.
// This starter emits a stable, replay-friendly record shape with redacted payloads; replace the logger sink if you need durable evidence storage.
public interface IExternalSystemEvidenceLogger
{
    Task CaptureAsync(
        string operationName,
        string correlationId,
        HttpRequestMessage request,
        string? rawRequestBody,
        HttpStatusCode statusCode,
        string? rawResponseBody,
        TimeSpan duration,
        CancellationToken cancellationToken);
}

public sealed record ExternalSystemEvidenceRecord(
    string OperationName,
    string CorrelationId,
    DateTimeOffset CapturedAtUtc,
    string Method,
    string? Uri,
    int StatusCode,
    long DurationMs,
    IReadOnlyDictionary<string, string> Headers,
    string RequestBodySha256,
    string ResponseBodySha256,
    string RequestBodyRedacted,
    string ResponseBodyRedacted);

public sealed class ExternalSystemEvidenceLogger : IExternalSystemEvidenceLogger
{
    private static readonly HashSet<string> SensitiveHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "ApiKey"
    };

    private static readonly string[] SensitiveFieldNames =
    [
        "name",
        "fullname",
        "username",
        "personname",
        "nationalid",
        "identitynumber",
        "idnumber",
        "idno",
        "passportnumber",
        "phone",
        "mobile",
        "email",
        "address",
        "token",
        "authorization",
        "apikey"
    ];

    private readonly ILogger<ExternalSystemEvidenceLogger> _logger;

    public ExternalSystemEvidenceLogger(ILogger<ExternalSystemEvidenceLogger> logger)
    {
        _logger = logger;
    }

    public Task CaptureAsync(
        string operationName,
        string correlationId,
        HttpRequestMessage request,
        string? rawRequestBody,
        HttpStatusCode statusCode,
        string? rawResponseBody,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        var safeHeaders = request.Headers
            .Where(header => !SensitiveHeaderNames.Contains(header.Key))
            .ToDictionary(
                header => header.Key,
                header => string.Join(",", header.Value),
                StringComparer.OrdinalIgnoreCase);

        var record = new ExternalSystemEvidenceRecord(
            operationName,
            correlationId,
            DateTimeOffset.UtcNow,
            request.Method.Method,
            request.RequestUri?.ToString(),
            (int)statusCode,
            (long)duration.TotalMilliseconds,
            safeHeaders,
            ComputeSha256(rawRequestBody),
            ComputeSha256(rawResponseBody),
            RedactAndTruncate(rawRequestBody),
            RedactAndTruncate(rawResponseBody));

        _logger.LogInformation(
            "External evidence captured: {@EvidenceRecord}",
            record);

        return Task.CompletedTask;
    }

    private static string RedactAndTruncate(string? rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return "<empty>";

        var normalized = rawBody
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        var redacted = SensitiveFieldNames.Aggregate(
            normalized,
            (current, fieldName) => RedactField(current, fieldName));

        const int maxLength = 512;
        return redacted.Length <= maxLength
            ? redacted
            : redacted[..maxLength] + "...(truncated)";
    }

    private static string RedactField(string rawBody, string fieldName)
    {
        var jsonPattern = $"(\"{Regex.Escape(fieldName)}\"\\s*:\\s*\")([^\"]*)(\")";
        var xmlPattern = $"(<{Regex.Escape(fieldName)}>)(.*?)(</{Regex.Escape(fieldName)}>)";

        var withJsonRedaction = Regex.Replace(
            rawBody,
            jsonPattern,
            "$1***REDACTED***$3",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return Regex.Replace(
            withJsonRedaction,
            xmlPattern,
            "$1***REDACTED***$3",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string ComputeSha256(string? rawBody)
    {
        var bytes = Encoding.UTF8.GetBytes(rawBody ?? string.Empty);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
