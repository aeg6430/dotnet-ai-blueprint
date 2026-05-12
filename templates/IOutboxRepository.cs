using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — repository for writing durable local outbox messages inside the main UoW.
public interface IOutboxRepository
{
    Task EnqueueAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<OutboxMessageRecord>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken);
    Task MarkProcessingAsync(string messageId, CancellationToken cancellationToken);
    Task MarkSucceededAsync(string messageId, CancellationToken cancellationToken);
    Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken);
}
