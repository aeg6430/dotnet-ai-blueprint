namespace Project.Core.DTOs;

// TEMPLATE — outbound write request that carries a stable request ID for retry safety.
public sealed record ShipmentCreateRequest(
    string RequestId,
    string OrderId,
    string CarrierCode,
    string DestinationPostalCode);
