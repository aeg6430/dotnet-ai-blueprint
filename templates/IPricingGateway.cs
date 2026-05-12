using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — read-only outbound lookup before the local UoW starts.
public interface IPricingGateway
{
    Task<PricingQuoteResponse?> GetQuoteAsync(
        PricingQuoteRequest request,
        CancellationToken cancellationToken);
}
