namespace Project.Infrastructure.Adapters;

// TEMPLATE — options bound from configuration for the typed HttpClient sample.
public sealed class InventoryGatewayOptions
{
    public const string SectionName = "InventoryApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 10;
    public string VerificationPath { get; init; } = "inventory/verify";
}
