using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — background worker that delivers committed outbox rows after the local transaction has finished.
public sealed class OutboxDeliveryWorker : BackgroundService
{
    private readonly IOutboxDispatcher _dispatcher;
    private readonly ILogger<OutboxDeliveryWorker> _logger;
    private readonly OutboxDeliveryOptions _options;

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
        await DispatchOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DispatchOnceAsync(stoppingToken);
        }
    }

    private async Task DispatchOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dispatcher.DispatchPendingAsync(_options.BatchSize, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Outbox delivery worker failed while dispatching committed messages.");
        }
    }
}
