using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — remote payment authorization is an idempotent outbound write with explicit header-based dedupe.
public sealed class PaymentGateway : BaseHttpAdapter, IPaymentGateway
{
    private const string IdempotencyHeaderName = "Idempotency-Key";
    private readonly PaymentGatewayOptions _options;

    public PaymentGateway(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<PaymentGateway> logger)
        : base(context, environment, httpClient, logger)
    {
        _options = options.Value;
    }

    public async Task<PaymentAuthorizeResponse?> AuthorizeAsync(
        PaymentAuthorizeRequest request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, _options.AuthorizePath)
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add(IdempotencyHeaderName, request.RequestId);

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            message.Headers.Add(_options.ApiKeyHeaderName, _options.ApiKey);

        return await SendAsync<PaymentAuthorizeResponse>(message, cancellationToken);
    }
}
