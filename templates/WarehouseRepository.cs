using Dapper;
using Microsoft.Extensions.Logging;
using Project.Infrastructure.Context;
using Project.Infrastructure.Entities;

namespace Project.Infrastructure.Repositories;

public class WarehouseRepository : BaseRepository, IWarehouseRepository
{
    private readonly ILogger<WarehouseRepository> _logger;

    public WarehouseRepository(IDapperContext context, ILogger<WarehouseRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public async Task UpdateStockAsync(int skuId, int quantity)
    {
        const string sql = @"
            UPDATE Inventory SET Stock = Stock - @Quantity 
            WHERE SkuId = @SkuId
        ";

        var parameters = new
        {
            SkuId = skuId,
            Quantity = quantity
        };

        try
        {
            _logger.LogDebug("Executing UpdateStockAsync for SkuId: {SkuId}, Quantity: {Quantity}", skuId, quantity);

            await Connection.ExecuteAsync(
                sql,
                parameters,
                Transaction,
                commandTimeout: DefaultCommandTimeoutSeconds);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating stock in database for SkuId: {SkuId}", skuId);
            throw;
        }
    }
}