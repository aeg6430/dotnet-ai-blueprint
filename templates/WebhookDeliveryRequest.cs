namespace Project.Core.DTOs;

// TEMPLATE — webhook delivery request written to the outbox and dispatched after commit.
public sealed record WebhookDeliveryRequest(
    string RequestId,
    string EventType,
    string PayloadJson);
