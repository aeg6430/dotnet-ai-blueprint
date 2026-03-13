// TEMPLATE — Use this pattern when the assistant or developer cannot implement a piece of logic.
// Never delete, comment out, or silently stub code. Follow this exact pattern instead.
//
// Steps:
//   1. Leave a detailed comment block explaining what/why/what's needed
//   2. throw new NotImplementedException("reason") — keeps structure intact, fails loudly at runtime
//
// ─────────────────────────────────────────────────────────────────────────────
// EXAMPLE USAGE:
// ─────────────────────────────────────────────────────────────────────────────

using Project.Core.DTOs;
using Project.Core.Interfaces.IServices;

namespace Project.Core.Services;

public class PricingService : IPricingService
{
    public Task<decimal> CalculateDynamicPriceAsync(int skuId, int quantity)
    {
        // NOT IMPLEMENTED - 2026/03/14
        //
        // What:
        //   Cannot calculate dynamic pricing because the pricing rules are not yet
        //   defined in the spec. The formula requires discount tiers which are missing
        //   from the current SkuDto.
        //
        // Why:
        //   Waiting for business confirmation on how tiered discounts interact with
        //   bulk orders. See docs/specs/pricing.md when available.
        //
        // What is needed to implement:
        //   - Discount tier table in the database
        //   - Pricing rules confirmed and documented in docs/specs/pricing.md
        //   - SkuDto updated to include discount tier reference
        //
        throw new NotImplementedException("Dynamic pricing not yet defined — see comment above");
    }
}
