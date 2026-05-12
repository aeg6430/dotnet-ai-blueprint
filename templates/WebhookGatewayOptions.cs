namespace Project.Infrastructure.Adapters;

// TEMPLATE — options bound from configuration for outbound webhook delivery.
public sealed class WebhookGatewayOptions
{
    public const string SectionName = "WebhookDelivery";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 10;
    public string DeliveryPath { get; init; } = "webhooks/outbox-delivery";
    public string SigningSecret { get; init; } = string.Empty;
}
