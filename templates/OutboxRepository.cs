using Dapper;
using Microsoft.Extensions.Logging;
using System.Linq;
using Project.Core.DTOs;
using Project.Core.Interfaces;
using Project.Infrastructure.Helpers;

namespace Project.Infrastructure.Repositories;

// TEMPLATE — writes durable local outbox rows inside the same explicit UoW as the business write.
public sealed class OutboxRepository : BaseRepository, IOutboxRepository
{
    private readonly IOutboxMessageSerializer _serializer;
    private readonly ILogger<OutboxRepository> _logger;

    public OutboxRepository(
        IDapperContext context,
        IOutboxMessageSerializer serializer,
        ILogger<OutboxRepository> logger)
        : base(context)
    {
        _serializer = serializer;
        _logger = logger;
    }

    public async Task EnqueueAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var envelope = _serializer.Serialize(message);

        const string sql = @"
            INSERT INTO OutboxMessages (MessageId, MessageType, Payload, Status, CreatedAtUtc)
            VALUES (@MessageId, @MessageType, @Payload, @Status, CURRENT_TIMESTAMP)
        ";

        var parameters = new
        {
            envelope.MessageId,
            envelope.MessageType,
            envelope.Payload,
            Status = "Pending"
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
            _logger.LogError(ex, "Error enqueuing outbox message {MessageType}", envelope.MessageType);
            throw;
        }
    }

    public async Task<IReadOnlyList<OutboxMessageRecord>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        const string sql = @"
            SELECT MessageId, MessageType, Payload, Status, CreatedAtUtc, AttemptCount, LastAttemptAtUtc, LastError
            FROM OutboxMessages
            WHERE Status = @Status
            ORDER BY CreatedAtUtc
        ";

        var rows = await Connection.QueryAsync<OutboxMessageRecord>(
            sql,
            new { Status = "Pending" },
            Transaction,
            commandTimeout: DefaultCommandTimeoutSeconds);

        return rows.Take(batchSize).ToArray();
    }

    public async Task MarkProcessingAsync(string messageId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        const string sql = @"
            UPDATE OutboxMessages
            SET Status = @Status,
                AttemptCount = AttemptCount + 1,
                LastAttemptAtUtc = CURRENT_TIMESTAMP,
                LastError = NULL
            WHERE MessageId = @MessageId
        ";

        await Connection.ExecuteAsync(
            sql,
            new
            {
                MessageId = messageId,
                Status = "Processing"
            },
            Transaction,
            commandTimeout: DefaultCommandTimeoutSeconds);
    }

    public async Task MarkSucceededAsync(string messageId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        const string sql = @"
            UPDATE OutboxMessages
            SET Status = @Status,
                ProcessedAtUtc = CURRENT_TIMESTAMP,
                LastError = NULL
            WHERE MessageId = @MessageId
        ";

        await Connection.ExecuteAsync(
            sql,
            new
            {
                MessageId = messageId,
                Status = "Succeeded"
            },
            Transaction,
            commandTimeout: DefaultCommandTimeoutSeconds);
    }

    public async Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        const string sql = @"
            UPDATE OutboxMessages
            SET Status = @Status,
                LastError = @LastError
            WHERE MessageId = @MessageId
        ";

        await Connection.ExecuteAsync(
            sql,
            new
            {
                MessageId = messageId,
                Status = "Failed",
                LastError = error
            },
            Transaction,
            commandTimeout: DefaultCommandTimeoutSeconds);
    }
}
