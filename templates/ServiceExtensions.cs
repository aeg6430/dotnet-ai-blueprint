// TEMPLATE — rename namespace to match your project, then add your own registrations.
// This is the canonical DI registration file. ALL registrations live here — never in Program.cs.
using Project.Api.Middlewares;
using Project.Core.Interfaces;
using Project.Infrastructure.Context;
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

        // Outbound adapters belong in Infrastructure and should use typed/named HttpClient registrations
        // with Polly-style timeout/retry/circuit-breaker policies. See docs/rules/resilience.md and
        // templates/BaseHttpAdapter.cs for the adapter pattern.
        // Example:
        // services.AddHttpClient<IInventoryGateway, InventoryGateway>(client =>
        //     {
        //         client.BaseAddress = new Uri(configuration["InventoryApi:BaseUrl"]!);
        //         client.Timeout = TimeSpan.FromSeconds(10);
        //     })
        //     .AddPolicyHandler(ResiliencePolicies.TimeoutPolicy)
        //     .AddPolicyHandler(ResiliencePolicies.RetryPolicy)
        //     .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);

        // --- Repositories ---
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockLedgerRepository, StockLedgerRepository>();

        // --- Services ---
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<StockTransferUseCase>();

        // --- Global Error Handling ---
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
