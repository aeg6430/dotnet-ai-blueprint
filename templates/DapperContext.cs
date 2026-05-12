using System.Data;
using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Context;

// TEMPLATE — scoped, lazy connection, explicit transaction lifecycle.
public sealed class DapperContext : IDapperContext
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly ILogger<DapperContext> _logger;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _isFaulted;

    public DapperContext(IDatabaseFactory databaseFactory, ILogger<DapperContext> logger)
    {
        _databaseFactory = databaseFactory;
        _logger = logger;
    }

    public IDbConnection Connection => _connection ??= OpenConnection();
    public IDbTransaction? Transaction => _transaction;
    public bool IsInTransaction => _transaction is not null;
    public int DefaultCommandTimeoutSeconds => 5;

    public void Begin()
    {
        if (_transaction is not null)
            return;

        if (_isFaulted)
            throw new InvalidOperationException("Cannot begin a faulted unit of work.");

        _transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        if (_isFaulted)
            throw new InvalidOperationException("Cannot commit a faulted unit of work.");

        if (_transaction is null)
            return;

        _transaction.Commit();
        DisposeTransaction();
    }

    public void Rollback()
    {
        if (_transaction is null)
            return;

        _isFaulted = true;

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            DisposeTransaction();
        }
    }

    public void EnsureNoActiveTransaction()
    {
        if (!IsInTransaction)
            return;

        throw new InvalidOperationException(
            "Remote IO must not run while the primary database transaction is active.");
    }

    public async Task ExecuteAsync(Func<Task> work, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ownsTransaction = !IsInTransaction;
        if (ownsTransaction)
            Begin();

        try
        {
            await work();
            if (ownsTransaction)
                Commit();
        }
        catch
        {
            // Fail-fast rollback invalidates the entire ambient UoW.
            Rollback();
            throw;
        }
    }

    public void Dispose()
    {
        if (IsInTransaction)
        {
            _logger.LogCritical(
                "DapperContext was disposed with an active transaction. Rolling back leaked unit of work.");
            Rollback();
        }

        _connection?.Dispose();
        _connection = null;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    private IDbConnection OpenConnection()
    {
        var connection = _databaseFactory.CreateConnection();
        connection.Open();
        return connection;
    }

    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }
}
