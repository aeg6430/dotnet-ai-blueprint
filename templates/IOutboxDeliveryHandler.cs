using Project.Core.DTOs;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — one handler per outbox message type keeps delivery routing explicit and testable.
public interface IOutboxDeliveryHandler
{
    string MessageType { get; }

    Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken);
}
