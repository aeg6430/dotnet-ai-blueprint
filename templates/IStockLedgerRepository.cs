namespace Project.Core.Interfaces;

// TEMPLATE — local write collaborator used by StockService for pure DB-only atomic work.
public interface IStockLedgerRepository
{
    Task AppendAsync(int skuId, int quantity, string reason);
}
