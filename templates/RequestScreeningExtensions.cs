using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Api.Middlewares;

public static class RequestScreeningExtensions
{
    public static IServiceCollection AddRequestScreening(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RequestScreeningOptions>(
            configuration.GetSection(RequestScreeningOptions.SectionName));

        return services;
    }

    public static IApplicationBuilder UseRequestScreening(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestScreeningMiddleware>();
    }
}
