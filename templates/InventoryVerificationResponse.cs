namespace Project.Core.DTOs;

// TEMPLATE — response contract returned by the outbound inventory verification gateway.
public sealed record InventoryVerificationResponse(bool IsAccepted, string? Reason = null);
