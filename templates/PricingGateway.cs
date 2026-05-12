using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — GET-style outbound lookup guarded by BaseHttpAdapter + Polly policies.
public sealed class PricingGateway : BaseHttpAdapter, IPricingGateway
{
    private readonly PricingGatewayOptions _options;

    public PricingGateway(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        IOptions<PricingGatewayOptions> options,
        ILogger<PricingGateway> logger)
        : base(context, environment, httpClient, logger)
    {
        _options = options.Value;
    }

    public async Task<PricingQuoteResponse?> GetQuoteAsync(
        PricingQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var relativeUri =
            $"{_options.QuotePath}?skuId={request.SkuId}&currencyCode={Uri.EscapeDataString(request.CurrencyCode)}";

        return await GetAsync<PricingQuoteResponse>(relativeUri, cancellationToken);
    }
}
