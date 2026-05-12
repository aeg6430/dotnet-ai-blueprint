using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — post-commit webhook delivery with explicit event headers and optional HMAC signing.
public sealed class WebhookGateway : BaseHttpAdapter, IWebhookGateway
{
    private const string DeliveryIdHeaderName = "X-Webhook-Id";
    private const string EventTypeHeaderName = "X-Webhook-Event";
    private const string SignatureHeaderName = "X-Webhook-Signature";
    private readonly WebhookGatewayOptions _options;

    public WebhookGateway(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        IOptions<WebhookGatewayOptions> options,
        ILogger<WebhookGateway> logger)
        : base(context, environment, httpClient, logger)
    {
        _options = options.Value;
    }

    public async Task<WebhookDeliveryResponse> DeliverAsync(
        WebhookDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, _options.DeliveryPath)
        {
            Content = new StringContent(request.PayloadJson, Encoding.UTF8, "application/json")
        };

        message.Headers.Add(DeliveryIdHeaderName, request.RequestId);
        message.Headers.Add(EventTypeHeaderName, request.EventType);

        if (!string.IsNullOrWhiteSpace(_options.SigningSecret))
            message.Headers.Add(SignatureHeaderName, ComputeSignature(request.PayloadJson, _options.SigningSecret));

        using var response = await SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();

        return new WebhookDeliveryResponse(
            request.RequestId,
            (int)response.StatusCode,
            response.StatusCode.ToString());
    }

    private static string ComputeSignature(string payloadJson, string signingSecret)
    {
        var key = Encoding.UTF8.GetBytes(signingSecret);
        var payload = Encoding.UTF8.GetBytes(payloadJson);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(payload));
    }
}
