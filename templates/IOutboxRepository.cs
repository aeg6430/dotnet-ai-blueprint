namespace Project.Core.Interfaces;

// TEMPLATE — repository for writing durable local outbox messages inside the main UoW.
public interface IOutboxRepository
{
    Task EnqueueAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
}
