using System.Data;

namespace Project.Core.Interfaces;

// TEMPLATE — keep the contract explicit so use cases can control short-lived local transactions.
public interface IDapperContext : IDisposable, IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    bool IsInTransaction { get; }
    int DefaultCommandTimeoutSeconds { get; }

    void Begin();
    void Commit();
    void Rollback();

    // Outbound adapters call this before remote IO to prevent pool starvation bugs.
    void EnsureNoActiveTransaction();

    // Optional helper for local atomic DB work only.
    Task ExecuteAsync(Func<Task> work, CancellationToken cancellationToken = default);
}
