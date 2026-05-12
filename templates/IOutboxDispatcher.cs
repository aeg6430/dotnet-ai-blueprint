namespace Project.Infrastructure.Adapters;

// TEMPLATE — dispatches committed outbox rows to concrete outbound handlers after commit.
public interface IOutboxDispatcher
{
    Task DispatchPendingAsync(int batchSize, CancellationToken cancellationToken);
}
