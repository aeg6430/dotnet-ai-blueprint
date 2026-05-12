namespace Acme.Core.Outbound;

public sealed record PaymentAuthorizeRequest(
    string RequestId,
    string OrderId,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethodToken);

public sealed record PaymentAuthorizeResponse(
    string PaymentId,
    string Status,
    bool RequiresCapture = false,
    string? AuthorizationCode = null,
    string? DeclineReason = null);

public sealed record WebhookDeliveryRequest(
    string RequestId,
    string EventType,
    string PayloadJson);

public sealed record WebhookDeliveryResponse(
    string DeliveryId,
    int StatusCode,
    string Status);

public sealed record MessagePublishRequest(
    string RequestId,
    string MessageType,
    string PartitionKey,
    string PayloadJson);

public sealed record OutboxMessageRecord(
    string MessageId,
    string MessageType,
    string Payload,
    string Status,
    DateTime CreatedAtUtc,
    int AttemptCount,
    DateTime? LastAttemptAtUtc = null,
    string? LastError = null);

public interface IPaymentGateway
{
    Task<PaymentAuthorizeResponse?> AuthorizeAsync(
        PaymentAuthorizeRequest request,
        CancellationToken cancellationToken);
}

public interface IWebhookGateway
{
    Task<WebhookDeliveryResponse> DeliverAsync(
        WebhookDeliveryRequest request,
        CancellationToken cancellationToken);
}

public interface IMessagePublisher
{
    Task PublishAsync(
        MessagePublishRequest request,
        CancellationToken cancellationToken);
}

public interface IOutboxRepository
{
    Task EnqueueAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<OutboxMessageRecord>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken);
    Task MarkProcessingAsync(string messageId, CancellationToken cancellationToken);
    Task MarkSucceededAsync(string messageId, CancellationToken cancellationToken);
    Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken);
}
