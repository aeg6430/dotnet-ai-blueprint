using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — generic post-commit dispatcher that routes committed outbox rows to registered delivery handlers.
public sealed class OutboxDispatcher : IOutboxDispatcher
{
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IReadOnlyDictionary<string, IOutboxDeliveryHandler> _handlers;

    public OutboxDispatcher(
        ILogger<OutboxDispatcher> logger,
        IOutboxRepository outboxRepository,
        IEnumerable<IOutboxDeliveryHandler> handlers)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _handlers = handlers.ToDictionary(handler => handler.MessageType, StringComparer.Ordinal);
    }

    public async Task DispatchPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var messages = await _outboxRepository.GetPendingBatchAsync(batchSize, cancellationToken);

        foreach (var message in messages)
        {
            if (!_handlers.TryGetValue(message.MessageType, out var handler))
            {
                const string template = "No outbox delivery handler is registered for message type {MessageType}.";
                _logger.LogError(template, message.MessageType);
                await _outboxRepository.MarkFailedAsync(
                    message.MessageId,
                    $"No handler registered for message type {message.MessageType}.",
                    cancellationToken);
                continue;
            }

            try
            {
                await _outboxRepository.MarkProcessingAsync(message.MessageId, cancellationToken);
                await handler.DeliverAsync(message, cancellationToken);
                await _outboxRepository.MarkSucceededAsync(message.MessageId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching outbox message {MessageId} ({MessageType})", message.MessageId, message.MessageType);
                await _outboxRepository.MarkFailedAsync(message.MessageId, ex.Message, cancellationToken);
            }
        }
    }
}
