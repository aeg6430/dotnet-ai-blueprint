using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Core.Services;

// TEMPLATE — service-oriented orchestration sample for remote verification + short local UoW + outbox.
public sealed class StockTransferService
{
    private readonly IDapperContext _context;
    private readonly ILogger<StockTransferService> _logger;
    private readonly ITransferIdempotencyRepository _idempotencyRepository;
    private readonly IInventoryGateway _inventoryGateway;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxRepository _outboxRepository;

    public StockTransferService(
        IDapperContext context,
        ILogger<StockTransferService> logger,
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

    public async Task TransferAsync(StockTransferDto dto, CancellationToken cancellationToken = default)
    {
        if (await _idempotencyRepository.ExistsAsync(dto.RequestId, cancellationToken))
        {
            _logger.LogInformation("Skipping duplicate transfer request {RequestId}", dto.RequestId);
            return;
        }

        var verification = await _inventoryGateway.VerifyAsync(
            new InventoryVerificationRequest(dto.RequestId, dto.SkuId, dto.Quantity),
            cancellationToken);

        if (verification is null || !verification.IsAccepted)
            throw new InvalidOperationException("Stock transfer was rejected by the remote inventory gateway.");

        await _context.ExecuteAsync(async () =>
        {
            await _warehouseRepository.UpdateStockAsync(dto.SkuId, dto.Quantity);
            await _idempotencyRepository.MarkCompletedAsync(dto.RequestId, cancellationToken);
            await _outboxRepository.EnqueueAsync(
                new StockTransferCommitted(dto.RequestId, dto.SkuId, dto.Quantity),
                cancellationToken);
        }, cancellationToken);
    }
}
