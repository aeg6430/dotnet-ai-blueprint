namespace Project.Core.DTOs;

// TEMPLATE — remote payment authorization result returned by the outbound payment gateway.
public sealed record PaymentAuthorizeResponse(
    string PaymentId,
    string Status,
    bool RequiresCapture = false,
    string? AuthorizationCode = null,
    string? DeclineReason = null);
