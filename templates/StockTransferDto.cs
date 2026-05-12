namespace Project.Core.DTOs;

// TEMPLATE — request model for examples that need idempotency plus stock movement data.
public sealed record StockTransferDto(string RequestId, int SkuId, int Quantity);
