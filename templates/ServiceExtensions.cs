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

        // DapperContext — Scoped so all repositories share the same connection/transaction per request
        services.AddScoped<IDapperContext, DapperContext>();

        // --- Repositories ---
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        // --- Services ---
        services.AddScoped<IStockService, StockService>();

        // --- Global Error Handling ---
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
