using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Core.Services;

// TEMPLATE — remote verification first, then short local transaction + outbox.
// This is the preferred shape when a use case needs external read/verification before local writes.
public sealed class StockTransferUseCase
{
    private readonly IDapperContext _context;
    private readonly ILogger<StockTransferUseCase> _logger;
    private readonly ITransferIdempotencyRepository _idempotencyRepository;
    private readonly IInventoryGateway _inventoryGateway;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxRepository _outboxRepository;

    public StockTransferUseCase(
        IDapperContext context,
        ILogger<StockTransferUseCase> logger,
        ITransferIdempotencyRepository idempotencyRepository,
        IInventoryGateway inventoryGateway,
        IWarehouseRepository warehouseRepository,
        IOutboxRepository outboxRepository)
    {
        _context = context;
        _logger = logger;
        _idempotencyRepository = idempotencyRepository;
        _inventoryGateway = inventoryGateway;
        _warehouseRepository = warehouseRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task HandleAsync(StockTransferDto dto, CancellationToken cancellationToken = default)
    {
        // Fast dedupe before remote IO keeps retries cheap.
        if (await _idempotencyRepository.ExistsAsync(dto.RequestId, cancellationToken))
        {
            _logger.LogInformation("Skipping duplicate stock transfer request {RequestId}", dto.RequestId);
            return;
        }

        // Remote verification happens before the local UoW starts.
        var verification = await _inventoryGateway.VerifyAsync(
            dto.RequestId,
            dto.SkuId,
            dto.Quantity,
            cancellationToken);

        if (!verification.IsAccepted)
            throw new InvalidOperationException("Remote inventory verification was rejected.");

        await _context.ExecuteAsync(async () =>
        {
            // Repeat the dedupe check inside the local transaction for retry safety.
            if (await _idempotencyRepository.ExistsAsync(dto.RequestId, cancellationToken))
                return;

            await _warehouseRepository.UpdateStockAsync(dto.SkuId, dto.Quantity);
            await _idempotencyRepository.MarkCompletedAsync(dto.RequestId, cancellationToken);

            // The cross-system side effect is dispatched later from the outbox.
            await _outboxRepository.EnqueueAsync(
                new StockTransferCommitted(dto.RequestId, dto.SkuId, dto.Quantity),
                cancellationToken);
        }, cancellationToken);
    }
}

public sealed record RemoteInventoryVerificationResult(bool IsAccepted);
public sealed record StockTransferCommitted(string RequestId, int SkuId, int Quantity);

public interface IInventoryGateway
{
    Task<RemoteInventoryVerificationResult> VerifyAsync(
        string requestId,
        int skuId,
        int quantity,
        CancellationToken cancellationToken);
}

public interface ITransferIdempotencyRepository
{
    Task<bool> ExistsAsync(string requestId, CancellationToken cancellationToken);
    Task MarkCompletedAsync(string requestId, CancellationToken cancellationToken);
}

public interface IOutboxRepository
{
    Task EnqueueAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
}
