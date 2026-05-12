namespace Project.Infrastructure.Adapters;

// TEMPLATE — options bound from configuration for an idempotent remote write gateway.
public sealed class ShipmentGatewayOptions
{
    public const string SectionName = "ShipmentApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 12;
    public string CreateShipmentPath { get; init; } = "shipments";
}
