using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — idempotent POST-style outbound write using a request header plus shared resilience policies.
public sealed class ShipmentGateway : BaseHttpAdapter, IShipmentGateway
{
    private const string IdempotencyHeaderName = "Idempotency-Key";
    private readonly ShipmentGatewayOptions _options;

    public ShipmentGateway(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        IOptions<ShipmentGatewayOptions> options,
        ILogger<ShipmentGateway> logger)
        : base(context, environment, httpClient, logger)
    {
        _options = options.Value;
    }

    public async Task<ShipmentCreateResponse?> CreateShipmentAsync(
        ShipmentCreateRequest request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, _options.CreateShipmentPath)
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add(IdempotencyHeaderName, request.RequestId);

        return await SendAsync<ShipmentCreateResponse>(message, cancellationToken);
    }
}
