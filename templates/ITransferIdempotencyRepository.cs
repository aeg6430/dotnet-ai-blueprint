namespace Project.Core.Interfaces;

// TEMPLATE — idempotency store for retry-safe write orchestration.
public interface ITransferIdempotencyRepository
{
    Task<bool> ExistsAsync(string requestId, CancellationToken cancellationToken);
    Task MarkCompletedAsync(string requestId, CancellationToken cancellationToken);
}
