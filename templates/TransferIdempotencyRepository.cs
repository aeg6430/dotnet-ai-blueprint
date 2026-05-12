using Dapper;
using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Repositories;

// TEMPLATE — local dedupe table used before remote IO and rechecked inside the short transaction.
public sealed class TransferIdempotencyRepository : BaseRepository, ITransferIdempotencyRepository
{
    private readonly ILogger<TransferIdempotencyRepository> _logger;

    public TransferIdempotencyRepository(
        IDapperContext context,
        ILogger<TransferIdempotencyRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public async Task<bool> ExistsAsync(string requestId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        const string sql = @"
            SELECT COUNT(1)
            FROM TransferIdempotency
            WHERE RequestId = @RequestId
        ";

        var parameters = new
        {
            RequestId = requestId
        };

        try
        {
            var count = await Connection.ExecuteScalarAsync<int>(
                sql,
                parameters,
                Transaction,
                commandTimeout: DefaultCommandTimeoutSeconds);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking idempotency record for request {RequestId}", requestId);
            throw;
        }
    }

    public async Task MarkCompletedAsync(string requestId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        const string sql = @"
            INSERT INTO TransferIdempotency (RequestId, CompletedAtUtc)
            SELECT @RequestId, CURRENT_TIMESTAMP
            WHERE NOT EXISTS (
                SELECT 1
                FROM TransferIdempotency
                WHERE RequestId = @RequestId
            )
        ";

        var parameters = new
        {
            RequestId = requestId
        };

        try
        {
            await Connection.ExecuteAsync(
                sql,
                parameters,
                Transaction,
                commandTimeout: DefaultCommandTimeoutSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking request {RequestId} as completed", requestId);
            throw;
        }
    }
}
