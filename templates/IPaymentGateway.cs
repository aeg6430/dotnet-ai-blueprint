using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — remote payment authorization happens behind a Core port with explicit idempotency.
public interface IPaymentGateway
{
    Task<PaymentAuthorizeResponse?> AuthorizeAsync(
        PaymentAuthorizeRequest request,
        CancellationToken cancellationToken);
}
