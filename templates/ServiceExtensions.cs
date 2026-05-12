// TEMPLATE — rename namespace to match your project, then add your own registrations.
// This is the canonical DI registration file. ALL registrations live here — never in Program.cs.
using System;
using Microsoft.Extensions.Options;
using Project.Api.Middlewares;
using Project.Core.Interfaces;
using Project.Infrastructure.Adapters;
using Project.Infrastructure.Context;
using Project.Infrastructure.Helpers;
using Project.Infrastructure.Repositories;
using Project.Core.Services;

namespace Project.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Factory — Singleton (connection string doesn't change)
        services.AddSingleton<IDatabaseFactory>(_ =>
            new DatabaseFactory(configuration.GetConnectionString("DefaultConnection")!));

        // DapperContext — Scoped so repositories share the same explicit UoW when the use case opens one.
        // The connection itself stays lazy until the first SQL call.
        services.AddScoped<IDapperContext, DapperContext>();

        // Outbound adapters live in Infrastructure and are registered as typed HttpClient clients.
        services.Configure<InventoryGatewayOptions>(
            configuration.GetSection(InventoryGatewayOptions.SectionName));
        services.Configure<PricingGatewayOptions>(
            configuration.GetSection(PricingGatewayOptions.SectionName));
        services.Configure<ShipmentGatewayOptions>(
            configuration.GetSection(ShipmentGatewayOptions.SectionName));
        services.Configure<PaymentGatewayOptions>(
            configuration.GetSection(PaymentGatewayOptions.SectionName));
        services.Configure<WebhookGatewayOptions>(
            configuration.GetSection(WebhookGatewayOptions.SectionName));
        services.Configure<MessagePublisherOptions>(
            configuration.GetSection(MessagePublisherOptions.SectionName));
        services.Configure<OutboxDeliveryOptions>(
            configuration.GetSection(OutboxDeliveryOptions.SectionName));

        services.AddHttpClient<IInventoryGateway, InventoryGateway>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<InventoryGatewayOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
            .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
            .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);

        services.AddHttpClient<IPricingGateway, PricingGateway>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PricingGatewayOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
            .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
            .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);

        services.AddHttpClient<IShipmentGateway, ShipmentGateway>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ShipmentGatewayOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
            .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
            .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);

        services.AddHttpClient<IPaymentGateway, PaymentGateway>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
            .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
            .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);

        services.AddHttpClient<IWebhookGateway, WebhookGateway>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<WebhookGatewayOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
            .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
            .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);

        // TEMPLATE — when a dependency is semantically hostile, register a full external integration firewall:
        // services.AddHttpClient<IExternalSystemGateway, ExternalSystemGateway>(client =>
        //     {
        //         client.BaseAddress = new Uri("https://replace-me/");
        //         client.Timeout = TimeSpan.FromSeconds(15);
        //     })
        //     .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
        //     .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
        //     .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);
        // services.AddSingleton<ExternalSystemSanitizer>();
        // services.AddSingleton<ExternalSystemTranslator>();
        // services.AddSingleton<IExternalSystemEvidenceLogger, ExternalSystemEvidenceLogger>();

        services.AddScoped<IMessagePublisher, MessagePublisher>();
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();
        services.AddScoped<IOutboxDeliveryHandler, PaymentOutboxDeliveryHandler>();
        services.AddScoped<IOutboxDeliveryHandler, WebhookOutboxDeliveryHandler>();
        services.AddScoped<IOutboxDeliveryHandler, MessagePublishOutboxDeliveryHandler>();
        services.AddHostedService<OutboxDeliveryWorker>();

        // --- Repositories ---
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockLedgerRepository, StockLedgerRepository>();
        services.AddScoped<ITransferIdempotencyRepository, TransferIdempotencyRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // --- Helpers ---
        services.AddSingleton<IOutboxMessageSerializer, OutboxMessageSerializer>();

        // --- Services ---
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<StockTransferService>();
        services.AddScoped<StockTransferUseCase>();

        // --- Global Error Handling ---
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddRequestScreening(configuration);

        return services;
    }
}
