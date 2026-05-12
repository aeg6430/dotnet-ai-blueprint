using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — broker publication uses the same no-active-transaction guard as HTTP adapters.
// Replace the placeholder publish block with your broker SDK of choice.
public sealed class MessagePublisher : IMessagePublisher
{
    private readonly IDapperContext _context;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<MessagePublisher> _logger;
    private readonly MessagePublisherOptions _options;

    public MessagePublisher(
        IDapperContext context,
        IHostEnvironment environment,
        IOptions<MessagePublisherOptions> options,
        ILogger<MessagePublisher> logger)
    {
        _context = context;
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(MessagePublishRequest request, CancellationToken cancellationToken)
    {
        EnsureSafeOutboundBoundary();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.PublishTimeoutSeconds));

        // TEMPLATE — replace this placeholder with a broker SDK send call (Kafka, Service Bus, RabbitMQ, etc.).
        await Task.Delay(TimeSpan.FromMilliseconds(10), timeoutCts.Token);

        _logger.LogInformation(
            "Published message {MessageType} to {BrokerName}/{TopicName} with partition key {PartitionKey}",
            request.MessageType,
            _options.BrokerName,
            _options.TopicName,
            request.PartitionKey);
    }

    private void EnsureSafeOutboundBoundary()
    {
        try
        {
            _context.EnsureNoActiveTransaction();
        }
        catch (InvalidOperationException ex)
        {
            if (_environment.IsDevelopment() || _environment.IsEnvironment("Test"))
                throw;

            _logger.LogCritical(
                ex,
                "Message publisher invoked while a database transaction is active. Failing fast to protect the pool.");
            throw;
        }
    }
}
