namespace Project.Core.DTOs;

// TEMPLATE — explicit read-only request model for a remote pricing lookup.
public sealed record PricingQuoteRequest(int SkuId, string CurrencyCode);
