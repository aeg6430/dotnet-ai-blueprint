using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — Core port for outbound inventory verification.
public interface IInventoryGateway
{
    Task<InventoryVerificationResponse?> VerifyAsync(
        InventoryVerificationRequest request,
        CancellationToken cancellationToken);
}
