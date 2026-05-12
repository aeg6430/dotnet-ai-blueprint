using System.Text.Json;
using System.Xml.Linq;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — keep foreign payload names and shapes in Infrastructure-only wire models.
// This example intentionally models a messy system that may return either JSON or XML.
public sealed record ExternalSystemWireEnvelope(
    string? ResultCode,
    string? ResultFlag,
    string? ResultMessage,
    ExternalSystemWirePayload? Payload)
{
    public static ExternalSystemWireEnvelope Parse(string rawBody, string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return new ExternalSystemWireEnvelope(null, null, "Empty response body.", null);

        return LooksLikeXml(mediaType, rawBody)
            ? ParseXml(rawBody)
            : ParseJson(rawBody);
    }

    private static bool LooksLikeXml(string? mediaType, string rawBody)
    {
        if (!string.IsNullOrWhiteSpace(mediaType) &&
            mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return rawBody.TrimStart().StartsWith("<", StringComparison.Ordinal);
    }

    private static ExternalSystemWireEnvelope ParseJson(string rawBody)
    {
        using var document = JsonDocument.Parse(rawBody);
        var root = document.RootElement;

        var payload = TryGetProperty(root, "payload", "data", "result") is { ValueKind: not JsonValueKind.Null } payloadElement
            ? new ExternalSystemWirePayload(
                TryReadString(payloadElement, "A01_USER_NM", "userName", "name"),
                TryReadString(payloadElement, "BirthDateText", "birthDate", "birth_date"),
                TryReadString(payloadElement, "StatusText", "status", "statusText"))
            : null;

        return new ExternalSystemWireEnvelope(
            TryReadString(root, "ResultCode", "resultCode", "code"),
            TryReadString(root, "ResultFlag", "success", "resultFlag", "flag"),
            TryReadString(root, "ResultMessage", "message", "resultMessage", "msg"),
            payload);
    }

    private static ExternalSystemWireEnvelope ParseXml(string rawBody)
    {
        var document = XDocument.Parse(rawBody);
        var root = document.Root ?? throw new FormatException("XML response has no root element.");
        var payload = root.Element("Payload") ?? root.Element("Data") ?? root.Element("Result");

        return new ExternalSystemWireEnvelope(
            ReadElementValue(root, "ResultCode", "Code"),
            ReadElementValue(root, "ResultFlag", "Success", "Flag"),
            ReadElementValue(root, "ResultMessage", "Message", "Msg"),
            payload is null
                ? null
                : new ExternalSystemWirePayload(
                    ReadElementValue(payload, "A01_USER_NM", "UserName", "Name"),
                    ReadElementValue(payload, "BirthDateText", "BirthDate", "Birth_Date"),
                    ReadElementValue(payload, "StatusText", "Status", "StatusText")));
    }

    private static JsonElement? TryGetProperty(JsonElement element, params string[] candidateNames)
    {
        foreach (var name in candidateNames)
        {
            if (element.TryGetProperty(name, out var value))
                return value;
        }

        return null;
    }

    private static string? TryReadString(JsonElement element, params string[] candidateNames)
    {
        foreach (var name in candidateNames)
        {
            if (!element.TryGetProperty(name, out var value))
                continue;

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => value.ToString()
            };
        }

        return null;
    }

    private static string? ReadElementValue(XElement element, params string[] candidateNames)
    {
        foreach (var name in candidateNames)
        {
            var child = element.Element(name);
            if (child is not null)
                return child.Value;
        }

        return null;
    }
}

public sealed record ExternalSystemWirePayload(
    string? A01_USER_NM,
    string? BirthDateText,
    string? StatusText);
