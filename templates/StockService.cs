using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;
using Project.Core.DTOs;

namespace Project.Core.Services;

// TEMPLATE — pure local multi-table update.
// Use this shape when the use case does not call external services while the UoW is active.
public sealed class StockService : IStockService
{
    private readonly IDapperContext _context;
    private readonly ILogger<StockService> _logger;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IStockLedgerRepository _stockLedgerRepository;

    public StockService(
        IDapperContext context,
        ILogger<StockService> logger,
        IWarehouseRepository warehouseRepository,
        IStockLedgerRepository stockLedgerRepository)
    {
        _context = context;
        _logger = logger;
        _warehouseRepository = warehouseRepository;
        _stockLedgerRepository = stockLedgerRepository;
    }

    public async Task TransferStockAsync(StockTransferDto dto)
    {
        _logger.LogInformation("Starting stock transfer for SkuId: {SkuId}", dto.SkuId);

        await _context.ExecuteAsync(async () =>
        {
            await _warehouseRepository.UpdateStockAsync(dto.SkuId, dto.Quantity);
            await _stockLedgerRepository.AppendAsync(
                dto.SkuId,
                dto.Quantity,
                "Stock transferred inside one local transaction.");
        });

        _logger.LogInformation("Stock transfer committed successfully for SkuId: {SkuId}", dto.SkuId);
    }
}