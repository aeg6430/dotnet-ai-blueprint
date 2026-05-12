using Acme.Core.Outbound;

namespace Acme.Core.Services;

// Runnable reference — queue cross-system side effects into the local outbox, then let a worker deliver them later.
public sealed class OutboundDispatchDemoService
{
    private readonly IOutboxRepository _outboxRepository;

    public OutboundDispatchDemoService(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public Task QueuePaymentAsync(PaymentAuthorizeRequest request, CancellationToken cancellationToken) =>
        _outboxRepository.EnqueueAsync(request, cancellationToken);

    public Task QueueWebhookAsync(WebhookDeliveryRequest request, CancellationToken cancellationToken) =>
        _outboxRepository.EnqueueAsync(request, cancellationToken);

    public Task QueueMessageAsync(MessagePublishRequest request, CancellationToken cancellationToken) =>
        _outboxRepository.EnqueueAsync(request, cancellationToken);
}
