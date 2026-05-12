using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — complete typed HttpClient adapter using BaseHttpAdapter + Polly policies.
public sealed class InventoryGateway : BaseHttpAdapter, IInventoryGateway
{
    private readonly InventoryGatewayOptions _options;

    public InventoryGateway(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        IOptions<InventoryGatewayOptions> options,
        ILogger<InventoryGateway> logger)
        : base(context, environment, httpClient, logger)
    {
        _options = options.Value;
    }

    public async Task<InventoryVerificationResponse?> VerifyAsync(
        InventoryVerificationRequest request,
        CancellationToken cancellationToken)
    {
        return await PostAsJsonAsync<InventoryVerificationRequest, InventoryVerificationResponse>(
            _options.VerificationPath,
            request,
            cancellationToken);
    }
}
