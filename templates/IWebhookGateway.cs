using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — Core port for post-commit webhook delivery.
public interface IWebhookGateway
{
    Task<WebhookDeliveryResponse> DeliverAsync(
        WebhookDeliveryRequest request,
        CancellationToken cancellationToken);
}
