using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — post-commit webhook delivery handler that deserializes the outbox payload and calls the HTTP gateway.
public sealed class WebhookOutboxDeliveryHandler : IOutboxDeliveryHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IWebhookGateway _webhookGateway;
    private readonly ILogger<WebhookOutboxDeliveryHandler> _logger;

    public WebhookOutboxDeliveryHandler(
        IWebhookGateway webhookGateway,
        ILogger<WebhookOutboxDeliveryHandler> logger)
    {
        _webhookGateway = webhookGateway;
        _logger = logger;
    }

    public string MessageType => nameof(WebhookDeliveryRequest);

    public async Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<WebhookDeliveryRequest>(message.Payload, SerializerOptions)
            ?? throw new InvalidOperationException("Outbox payload could not be deserialized as WebhookDeliveryRequest.");

        var response = await _webhookGateway.DeliverAsync(request, cancellationToken);

        _logger.LogInformation(
            "Delivered webhook outbox message {MessageId} with downstream status {StatusCode}",
            message.MessageId,
            response.StatusCode);
    }
}
