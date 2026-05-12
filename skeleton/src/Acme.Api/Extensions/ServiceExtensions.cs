using Acme.Core.Outbound;
using Acme.Core.Services;
using Acme.Infrastructure.Outbound;
using Microsoft.Extensions.Options;

namespace Acme.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddOutboundDemo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentGatewayOptions>(configuration.GetSection(PaymentGatewayOptions.SectionName));
        services.Configure<WebhookGatewayOptions>(configuration.GetSection(WebhookGatewayOptions.SectionName));
        services.Configure<MessagePublisherOptions>(configuration.GetSection(MessagePublisherOptions.SectionName));
        services.Configure<OutboxDeliveryOptions>(configuration.GetSection(OutboxDeliveryOptions.SectionName));

        services.AddSingleton<IOutboxMessageSerializer, OutboxMessageSerializer>();
        services.AddSingleton<IOutboxRepository, InMemoryOutboxRepository>();

        services.AddHttpClient<IPaymentGateway, PaymentGateway>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddHttpClient<IWebhookGateway, WebhookGateway>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<WebhookGatewayOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddSingleton<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<IOutboxDeliveryHandler, PaymentOutboxDeliveryHandler>();
        services.AddSingleton<IOutboxDeliveryHandler, WebhookOutboxDeliveryHandler>();
        services.AddSingleton<IOutboxDeliveryHandler, MessagePublishOutboxDeliveryHandler>();
        services.AddSingleton<IOutboxDispatcher, OutboxDispatcher>();
        services.AddHostedService<OutboxDeliveryWorker>();

        services.AddScoped<OutboundDispatchDemoService>();

        return services;
    }
}
