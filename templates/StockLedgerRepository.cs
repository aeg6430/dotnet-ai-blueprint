using Dapper;
using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Repositories;

// TEMPLATE — companion repository for pure local multi-table write examples.
public sealed class StockLedgerRepository : BaseRepository, IStockLedgerRepository
{
    private readonly ILogger<StockLedgerRepository> _logger;

    public StockLedgerRepository(IDapperContext context, ILogger<StockLedgerRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public async Task AppendAsync(int skuId, int quantity, string reason)
    {
        const string sql = @"
            INSERT INTO StockLedger (SkuId, Quantity, Reason)
            VALUES (@SkuId, @Quantity, @Reason)
        ";

        var parameters = new
        {
            SkuId = skuId,
            Quantity = quantity,
            Reason = reason
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
            _logger.LogError(ex, "Error appending stock ledger entry for SkuId: {SkuId}", skuId);
            throw;
        }
    }
}
