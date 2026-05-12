using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — post-commit broker publish handler that keeps messaging delivery outside the main UoW.
public sealed class MessagePublishOutboxDeliveryHandler : IOutboxDeliveryHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<MessagePublishOutboxDeliveryHandler> _logger;

    public MessagePublishOutboxDeliveryHandler(
        IMessagePublisher messagePublisher,
        ILogger<MessagePublishOutboxDeliveryHandler> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public string MessageType => nameof(MessagePublishRequest);

    public async Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<MessagePublishRequest>(message.Payload, SerializerOptions)
            ?? throw new InvalidOperationException("Outbox payload could not be deserialized as MessagePublishRequest.");

        await _messagePublisher.PublishAsync(request, cancellationToken);

        _logger.LogInformation(
            "Published broker outbox message {MessageId} as {MessageType}",
            message.MessageId,
            request.MessageType);
    }
}
