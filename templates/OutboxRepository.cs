using Dapper;
using Microsoft.Extensions.Logging;
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
}
