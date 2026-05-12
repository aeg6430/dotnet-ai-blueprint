namespace Project.Infrastructure.Adapters;

// TEMPLATE — options bound from configuration for the outbound payment authorization gateway.
public sealed class PaymentGatewayOptions
{
    public const string SectionName = "PaymentApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 12;
    public string AuthorizePath { get; init; } = "payments/authorize";
    public string ApiKeyHeaderName { get; init; } = "X-Api-Key";
    public string ApiKey { get; init; } = string.Empty;
}
