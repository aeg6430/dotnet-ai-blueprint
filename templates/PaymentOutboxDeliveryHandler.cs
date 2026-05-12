using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — post-commit payment delivery handler that reuses the typed payment gateway.
public sealed class PaymentOutboxDeliveryHandler : IOutboxDeliveryHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentOutboxDeliveryHandler> _logger;

    public PaymentOutboxDeliveryHandler(
        IPaymentGateway paymentGateway,
        ILogger<PaymentOutboxDeliveryHandler> logger)
    {
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public string MessageType => nameof(PaymentAuthorizeRequest);

    public async Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<PaymentAuthorizeRequest>(message.Payload, SerializerOptions)
            ?? throw new InvalidOperationException("Outbox payload could not be deserialized as PaymentAuthorizeRequest.");

        var response = await _paymentGateway.AuthorizeAsync(request, cancellationToken)
            ?? throw new InvalidOperationException("Payment gateway returned no response for the outbox payment request.");

        _logger.LogInformation(
            "Delivered payment outbox message {MessageId} with status {Status}",
            message.MessageId,
            response.Status);
    }
}
