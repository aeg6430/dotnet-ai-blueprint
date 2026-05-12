using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Project.Api.Middlewares;

public sealed class RequestScreeningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<RequestScreeningOptions> _optionsMonitor;
    private readonly ILogger<RequestScreeningMiddleware> _logger;

    public RequestScreeningMiddleware(
        RequestDelegate next,
        IOptionsMonitor<RequestScreeningOptions> optionsMonitor,
        ILogger<RequestScreeningMiddleware> logger)
    {
        _next = next;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!options.Enabled)
        {
            await _next(context);
            return;
        }

        var request = context.Request;
        if (IsAllowlisted(request.Path, options))
        {
            await _next(context);
            return;
        }

        if (!TryMatch(request, options, out var matchType, out var configuredValue))
        {
            await _next(context);
            return;
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["RequestPath"] = request.Path.Value,
            ["RequestMethod"] = request.Method,
            ["ScreeningMode"] = options.ReportOnly ? "report-only" : "blocking",
            ["MatchType"] = matchType
        });

        _logger.LogWarning(
            "Request screening matched {MatchType} rule '{ConfiguredValue}' for {Method} {Path}.",
            matchType,
            configuredValue,
            request.Method,
            request.Path.Value);

        if (options.ReportOnly)
        {
            await _next(context);
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Status = options.BlockStatusCode,
            Type = options.ProblemType,
            Title = options.ProblemTitle,
            Detail = options.ProblemDetail,
            Instance = request.Path
        };

        problemDetails.Extensions["reasonCode"] = "request_screening";
        problemDetails.Extensions["matchType"] = matchType;

        context.Response.StatusCode = options.BlockStatusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static bool TryMatch(
        HttpRequest request,
        RequestScreeningOptions options,
        out string matchType,
        out string configuredValue)
    {
        if (TryMatchBlockedPath(request, options, out configuredValue))
        {
            matchType = "path";
            return true;
        }

        if (TryMatchBlockedQueryKey(request, options, out configuredValue))
        {
            matchType = "query-key";
            return true;
        }

        if (TryMatchBlockedQueryValueFragment(request, options, out configuredValue))
        {
            matchType = "query-fragment";
            return true;
        }

        matchType = string.Empty;
        configuredValue = string.Empty;
        return false;
    }

    private static bool TryMatchBlockedPath(
        HttpRequest request,
        RequestScreeningOptions options,
        out string configuredValue)
    {
        foreach (var pathPrefix in options.BlockedPathPrefixes.Where(HasValue))
        {
            if (!request.Path.StartsWithSegments(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (options.BlockedMethods.Length > 0 &&
                !options.BlockedMethods.Any(method =>
                    string.Equals(method, request.Method, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            configuredValue = pathPrefix;
            return true;
        }

        configuredValue = string.Empty;
        return false;
    }

    private static bool TryMatchBlockedQueryKey(
        HttpRequest request,
        RequestScreeningOptions options,
        out string configuredValue)
    {
        foreach (var queryKey in options.BlockedQueryKeys.Where(HasValue))
        {
            if (!request.Query.ContainsKey(queryKey))
            {
                continue;
            }

            configuredValue = queryKey;
            return true;
        }

        configuredValue = string.Empty;
        return false;
    }

    private static bool TryMatchBlockedQueryValueFragment(
        HttpRequest request,
        RequestScreeningOptions options,
        out string configuredValue)
    {
        foreach (var fragment in options.BlockedQueryValueFragments.Where(HasValue))
        {
            foreach (var queryValues in request.Query.Values)
            {
                if (queryValues.Any(value =>
                        value.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                {
                    configuredValue = fragment;
                    return true;
                }
            }
        }

        configuredValue = string.Empty;
        return false;
    }

    private static bool IsAllowlisted(
        PathString requestPath,
        RequestScreeningOptions options)
    {
        return options.AllowlistedPathPrefixes
            .Where(HasValue)
            .Any(pathPrefix =>
                requestPath.StartsWithSegments(pathPrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasValue(string? value) => !string.IsNullOrWhiteSpace(value);
}
