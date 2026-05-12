namespace Project.Core.DTOs;

// TEMPLATE — remote shipment creation response returned by the outbound gateway.
public sealed record ShipmentCreateResponse(
    string ShipmentId,
    string Status,
    string? TrackingNumber = null);
