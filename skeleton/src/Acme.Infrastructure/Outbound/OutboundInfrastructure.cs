using System.Text.Json;
using Acme.Core.Outbound;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Acme.Infrastructure.Outbound;

public sealed class PaymentGatewayOptions
{
    public const string SectionName = "PaymentApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 12;
    public string AuthorizePath { get; init; } = "payments/authorize";
}

public sealed class WebhookGatewayOptions
{
    public const string SectionName = "WebhookDelivery";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 10;
    public string DeliveryPath { get; init; } = "webhooks/outbox-delivery";
}

public sealed class MessagePublisherOptions
{
    public const string SectionName = "MessagePublisher";

    public string BrokerName { get; init; } = "DemoBroker";
    public string TopicName { get; init; } = "domain-events";
    public int PublishTimeoutSeconds { get; init; } = 8;
}

public sealed class OutboxDeliveryOptions
{
    public const string SectionName = "OutboxDelivery";

    public int BatchSize { get; init; } = 20;
    public int PollIntervalSeconds { get; init; } = 5;
}

public interface IOutboxDeliveryHandler
{
    string MessageType { get; }

    Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken);
}

public interface IOutboxDispatcher
{
    Task DispatchPendingAsync(int batchSize, CancellationToken cancellationToken);
}

public sealed record OutboxEnvelope(string MessageId, string MessageType, string Payload);

public interface IOutboxMessageSerializer
{
    OutboxEnvelope Serialize<TMessage>(TMessage message);
}

public sealed class OutboxMessageSerializer : IOutboxMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxEnvelope Serialize<TMessage>(TMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var runtimeType = message.GetType();
        var payload = JsonSerializer.Serialize(message, runtimeType, SerializerOptions);
        var messageType = runtimeType.Name;
        var requestId = runtimeType.GetProperty("RequestId")?.GetValue(message) as string;
        var messageId = string.IsNullOrWhiteSpace(requestId)
            ? Guid.NewGuid().ToString("N")
            : $"{messageType}:{requestId}";

        return new OutboxEnvelope(messageId, messageType, payload);
    }
}

public sealed class InMemoryOutboxRepository : IOutboxRepository
{
    private readonly object _gate = new();
    private readonly IOutboxMessageSerializer _serializer;
    private readonly List<OutboxMessageRecord> _messages = [];

    public InMemoryOutboxRepository(IOutboxMessageSerializer serializer)
    {
        _serializer = serializer;
    }

    public Task EnqueueAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var envelope = _serializer.Serialize(message);

        lock (_gate)
        {
            _messages.Add(new OutboxMessageRecord(
                envelope.MessageId,
                envelope.MessageType,
                envelope.Payload,
                "Pending",
                DateTime.UtcNow,
                0));
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessageRecord>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        IReadOnlyList<OutboxMessageRecord> batch;

        lock (_gate)
        {
            batch = _messages
                .Where(message => string.Equals(message.Status, "Pending", StringComparison.Ordinal))
                .OrderBy(message => message.CreatedAtUtc)
                .Take(batchSize)
                .ToArray();
        }

        return Task.FromResult(batch);
    }

    public Task MarkProcessingAsync(string messageId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        Update(messageId, message => message with
        {
            Status = "Processing",
            AttemptCount = message.AttemptCount + 1,
            LastAttemptAtUtc = DateTime.UtcNow,
            LastError = null
        });

        return Task.CompletedTask;
    }

    public Task MarkSucceededAsync(string messageId, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        Update(messageId, message => message with
        {
            Status = "Succeeded",
            LastError = null
        });

        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        Update(messageId, message => message with
        {
            Status = "Failed",
            LastError = error
        });

        return Task.CompletedTask;
    }

    private void Update(string messageId, Func<OutboxMessageRecord, OutboxMessageRecord> apply)
    {
        lock (_gate)
        {
            var index = _messages.FindIndex(message => string.Equals(message.MessageId, messageId, StringComparison.Ordinal));
            if (index >= 0)
                _messages[index] = apply(_messages[index]);
        }
    }
}

// Runnable reference — keep typed HttpClient registrations, but simulate the downstream result so the skeleton stays self-contained.
public sealed class PaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly PaymentGatewayOptions _options;
    private readonly ILogger<PaymentGateway> _logger;

    public PaymentGateway(
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<PaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<PaymentAuthorizeResponse?> AuthorizeAsync(PaymentAuthorizeRequest request, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        _logger.LogInformation(
            "Simulating payment authorization to {BaseUrl}{Path} for order {OrderId}",
            _httpClient.BaseAddress,
            _options.AuthorizePath,
            request.OrderId);

        return Task.FromResult<PaymentAuthorizeResponse?>(new PaymentAuthorizeResponse(
            PaymentId: $"pay_{request.RequestId}",
            Status: "Authorized",
            RequiresCapture: true,
            AuthorizationCode: "AUTH-OK"));
    }
}

public sealed class WebhookGateway : IWebhookGateway
{
    private readonly HttpClient _httpClient;
    private readonly WebhookGatewayOptions _options;
    private readonly ILogger<WebhookGateway> _logger;

    public WebhookGateway(
        HttpClient httpClient,
        IOptions<WebhookGatewayOptions> options,
        ILogger<WebhookGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<WebhookDeliveryResponse> DeliverAsync(WebhookDeliveryRequest request, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        _logger.LogInformation(
            "Simulating webhook delivery to {BaseUrl}{Path} for event {EventType}",
            _httpClient.BaseAddress,
            _options.DeliveryPath,
            request.EventType);

        return Task.FromResult(new WebhookDeliveryResponse(
            DeliveryId: request.RequestId,
            StatusCode: 202,
            Status: "Accepted"));
    }
}

public sealed class MessagePublisher : IMessagePublisher
{
    private readonly MessagePublisherOptions _options;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(
        IOptions<MessagePublisherOptions> options,
        ILogger<MessagePublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(MessagePublishRequest request, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.PublishTimeoutSeconds));

        await Task.Delay(TimeSpan.FromMilliseconds(10), timeoutCts.Token);

        _logger.LogInformation(
            "Simulating broker publish of {MessageType} to {BrokerName}/{TopicName}",
            request.MessageType,
            _options.BrokerName,
            _options.TopicName);
    }
}

public sealed class PaymentOutboxDeliveryHandler : IOutboxDeliveryHandler
{
    private readonly IPaymentGateway _paymentGateway;

    public PaymentOutboxDeliveryHandler(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public string MessageType => nameof(PaymentAuthorizeRequest);

    public async Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<PaymentAuthorizeRequest>(message.Payload)
            ?? throw new InvalidOperationException("Could not deserialize payment outbox payload.");

        _ = await _paymentGateway.AuthorizeAsync(request, cancellationToken);
    }
}

public sealed class WebhookOutboxDeliveryHandler : IOutboxDeliveryHandler
{
    private readonly IWebhookGateway _webhookGateway;

    public WebhookOutboxDeliveryHandler(IWebhookGateway webhookGateway)
    {
        _webhookGateway = webhookGateway;
    }

    public string MessageType => nameof(WebhookDeliveryRequest);

    public async Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<WebhookDeliveryRequest>(message.Payload)
            ?? throw new InvalidOperationException("Could not deserialize webhook outbox payload.");

        _ = await _webhookGateway.DeliverAsync(request, cancellationToken);
    }
}

public sealed class MessagePublishOutboxDeliveryHandler : IOutboxDeliveryHandler
{
    private readonly IMessagePublisher _messagePublisher;

    public MessagePublishOutboxDeliveryHandler(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public string MessageType => nameof(MessagePublishRequest);

    public Task DeliverAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<MessagePublishRequest>(message.Payload)
            ?? throw new InvalidOperationException("Could not deserialize message publish outbox payload.");

        return _messagePublisher.PublishAsync(request, cancellationToken);
    }
}

public sealed class OutboxDispatcher : IOutboxDispatcher
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IReadOnlyDictionary<string, IOutboxDeliveryHandler> _handlers;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(
        IOutboxRepository outboxRepository,
        IEnumerable<IOutboxDeliveryHandler> handlers,
        ILogger<OutboxDispatcher> logger)
    {
        _outboxRepository = outboxRepository;
        _logger = logger;
        _handlers = handlers.ToDictionary(handler => handler.MessageType, StringComparer.Ordinal);
    }

    public async Task DispatchPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pendingMessages = await _outboxRepository.GetPendingBatchAsync(batchSize, cancellationToken);

        foreach (var message in pendingMessages)
        {
            if (!_handlers.TryGetValue(message.MessageType, out var handler))
            {
                await _outboxRepository.MarkFailedAsync(message.MessageId, $"No delivery handler for {message.MessageType}", cancellationToken);
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
                _logger.LogError(ex, "Failed to deliver outbox message {MessageId}", message.MessageId);
                await _outboxRepository.MarkFailedAsync(message.MessageId, ex.Message, cancellationToken);
            }
        }
    }
}

public sealed class OutboxDeliveryWorker : BackgroundService
{
    private readonly IOutboxDispatcher _dispatcher;
    private readonly OutboxDeliveryOptions _options;
    private readonly ILogger<OutboxDeliveryWorker> _logger;

    public OutboxDeliveryWorker(
        IOutboxDispatcher dispatcher,
        IOptions<OutboxDeliveryOptions> options,
        ILogger<OutboxDeliveryWorker> logger)
    {
        _dispatcher = dispatcher;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _dispatcher.DispatchPendingAsync(_options.BatchSize, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox delivery worker iteration failed.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}
