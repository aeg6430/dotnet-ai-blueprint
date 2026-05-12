using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — idempotent outbound write behind a Core port.
public interface IShipmentGateway
{
    Task<ShipmentCreateResponse?> CreateShipmentAsync(
        ShipmentCreateRequest request,
        CancellationToken cancellationToken);
}
