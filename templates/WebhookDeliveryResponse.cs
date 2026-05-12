namespace Project.Core.DTOs;

// TEMPLATE — minimal webhook delivery result so the worker can log downstream acceptance details.
public sealed record WebhookDeliveryResponse(
    string DeliveryId,
    int StatusCode,
    string Status);
