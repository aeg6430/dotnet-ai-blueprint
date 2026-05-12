namespace Project.Infrastructure.Adapters;

// TEMPLATE — options bound from configuration for the read-only pricing lookup gateway.
public sealed class PricingGatewayOptions
{
    public const string SectionName = "PricingApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 8;
    public string QuotePath { get; init; } = "pricing/quote";
}
