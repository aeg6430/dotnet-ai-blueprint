namespace Project.Core.DTOs;

// TEMPLATE — explicit request model for outbound inventory verification.
public sealed record InventoryVerificationRequest(string RequestId, int SkuId, int Quantity);
