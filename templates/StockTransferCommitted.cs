namespace Project.Core.DTOs;

// TEMPLATE — outbox message emitted after the local stock transfer transaction commits.
public sealed record StockTransferCommitted(string RequestId, int SkuId, int Quantity);
