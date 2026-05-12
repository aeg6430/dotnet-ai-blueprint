using System.Net;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — semantic normalization belongs here, not in Core services.
// Replace the placeholder outcome mapping with the vocabulary that fits your project.
public enum ExternalSystemOutcome
{
    Success,
    NotFound,
    Rejected,
    DependencyFailure,
    Unknown
}

public sealed record ExternalSystemNormalizedPayload(
    ExternalSystemOutcome Outcome,
    string? UserName,
    DateOnly? BirthDate,
    string? Message);

public sealed class ExternalSystemTranslator
{
    private readonly ExternalSystemSanitizer _sanitizer;

    public ExternalSystemTranslator(ExternalSystemSanitizer sanitizer)
    {
        _sanitizer = sanitizer;
    }

    public ExternalSystemNormalizedPayload Translate(HttpStatusCode httpStatusCode, ExternalSystemWireEnvelope envelope)
    {
        var outcome = DetermineOutcome(httpStatusCode, envelope);
        return new ExternalSystemNormalizedPayload(
            outcome,
            NormalizeUserName(envelope.Payload?.A01_USER_NM),
            _sanitizer.ParseExternalDate(envelope.Payload?.BirthDateText),
            NormalizeMessage(envelope.ResultMessage));
    }

    public ExternalSystemOutcome DetermineOutcome(HttpStatusCode httpStatusCode, ExternalSystemWireEnvelope envelope)
    {
        if ((int)httpStatusCode >= 500)
            return ExternalSystemOutcome.DependencyFailure;

        if (httpStatusCode is HttpStatusCode.NotFound)
            return ExternalSystemOutcome.NotFound;

        if (httpStatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return ExternalSystemOutcome.Rejected;

        var code = _sanitizer.NormalizeToken(envelope.ResultCode);
        var flag = _sanitizer.NormalizeToken(envelope.ResultFlag);
        var message = _sanitizer.NormalizeToken(envelope.ResultMessage);
        var payloadStatus = _sanitizer.NormalizeToken(envelope.Payload?.StatusText);

        var successSignal = HasAny(code, flag, payloadStatus, message, "success", "ok", "accepted", "completed", "true");
        var notFoundSignal = HasAny(code, flag, payloadStatus, message, "notfound", "nodata", "empty", "missing");
        var rejectedSignal = HasAny(code, flag, payloadStatus, message, "forbidden", "rejected", "invalid", "denied", "unauthorized");
        var failureSignal = HasAny(code, flag, payloadStatus, message, "fail", "failed", "error", "exception");

        if (CountTrue(successSignal, notFoundSignal, rejectedSignal, failureSignal) > 1)
            return ExternalSystemOutcome.Unknown;

        if (notFoundSignal)
            return ExternalSystemOutcome.NotFound;

        if (rejectedSignal)
            return ExternalSystemOutcome.Rejected;

        if (failureSignal)
            return ExternalSystemOutcome.DependencyFailure;

        if (successSignal && envelope.Payload is not null)
            return ExternalSystemOutcome.Success;

        if (httpStatusCode is HttpStatusCode.OK && envelope.Payload is not null)
            return ExternalSystemOutcome.Success;

        return ExternalSystemOutcome.Unknown;
    }

    public string? NormalizeUserName(string? rawValue)
    {
        var text = _sanitizer.NormalizeFreeText(rawValue);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text;
    }

    public string? NormalizeMessage(string? rawValue)
    {
        var text = _sanitizer.NormalizeFreeText(rawValue);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text;
    }

    private static bool HasAny(params string[] valuesAndCandidates)
    {
        if (valuesAndCandidates.Length < 2)
            return false;

        var values = valuesAndCandidates[..4];
        var candidates = valuesAndCandidates[4..];
        return values.Any(value => !string.IsNullOrWhiteSpace(value) && candidates.Any(value.Contains));
    }

    private static int CountTrue(params bool[] values) => values.Count(v => v);
}
