using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — outbound adapters live in Infrastructure and guard against transaction overlap.
public abstract class BaseHttpAdapter
{
    private readonly IDapperContext _context;
    private readonly IHostEnvironment _environment;
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;

    protected BaseHttpAdapter(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        ILogger logger)
    {
        _context = context;
        _environment = environment;
        _httpClient = httpClient;
        _logger = logger;
    }

    protected async Task<TResponse?> GetAsync<TResponse>(string relativeUri, CancellationToken cancellationToken)
    {
        EnsureSafeOutboundBoundary();
        return await _httpClient.GetFromJsonAsync<TResponse>(relativeUri, cancellationToken);
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<TRequest>(
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken)
    {
        EnsureSafeOutboundBoundary();
        return await _httpClient.PostAsJsonAsync(relativeUri, payload, cancellationToken);
    }

    protected async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken)
    {
        var response = await PostAsJsonAsync(relativeUri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    private void EnsureSafeOutboundBoundary()
    {
        try
        {
            _context.EnsureNoActiveTransaction();
        }
        catch (InvalidOperationException ex)
        {
            if (_environment.IsDevelopment() || _environment.IsEnvironment("Test"))
                throw;

            _logger.LogCritical(
                ex,
                "Outbound adapter invoked while a database transaction is active. Failing fast to protect the pool.");
            throw;
        }
    }
}
