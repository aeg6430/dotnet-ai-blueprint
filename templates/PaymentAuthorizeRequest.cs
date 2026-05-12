namespace Project.Core.DTOs;

// TEMPLATE — retry-safe payment authorization request with a stable request ID.
public sealed record PaymentAuthorizeRequest(
    string RequestId,
    string OrderId,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethodToken);
