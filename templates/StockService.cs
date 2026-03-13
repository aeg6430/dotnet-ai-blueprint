using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;
using Project.Core.DTOs;

namespace Project.Core.Services;

public class StockService : IStockService
{
    private readonly IDapperContext _context;
    private readonly ILogger<StockService> _logger;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IAuditRepository _auditRepository;
   

    public StockService(
        IDapperContext context,
        ILogger<StockService> logger,
        IWarehouseRepository warehouseRepository,
        IAuditRepository auditRepository
     )
    {
        _context = context;
        _logger = logger;
        _warehouseRepository = warehouseRepository;
        _auditRepository = auditRepository;   
    }

    public async Task TransferStockAsync(StockTransferDto dto)
    {
        _logger.LogInformation("Starting stock transfer for SkuId: {SkuId}", dto.SkuId);

        // 1. Start Transaction boundary in the Service
        _context.Begin();

        try
        {
            // 2. Perform operations sharing the same 'DapperContext'
            await _warehouseRepository.UpdateStockAsync(dto.SkuId, dto.Quantity);
            await _auditRepository.LogChangeAsync($"Transferred {dto.Quantity} units");

            // 3. Commit if all succeed
            _context.Commit();
            _logger.LogInformation("Stock transfer committed successfully for SkuId: {SkuId}", dto.SkuId);
        }
        catch (Exception e)
        {
            // 4. Rollback the entire unit of work on failure
            _context.Rollback();
            _logger.LogError(e, "Stock transfer failed for SkuId: {SkuId}. Transaction rolled back.", dto.SkuId);
            throw;
        }
    }
}