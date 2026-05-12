using System;

namespace Project.Core.DTOs;

// TEMPLATE — a read-only quote returned by a remote pricing dependency.
public sealed record PricingQuoteResponse(
    int SkuId,
    string CurrencyCode,
    decimal UnitPrice,
    string QuoteId,
    DateTimeOffset ExpiresAtUtc);
