using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project.Core.DTOs;
using Project.Core.Interfaces;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — use this shape when the dependency is semantically hostile, not just syntactically different.
// The adapter keeps raw transport concerns, wire models, and normalization logic in Infrastructure.
public sealed class ExternalSystemGateway : BaseHttpAdapter, IExternalSystemGateway
{
    private const string CorrelationHeaderName = "X-Correlation-Id";
    private const string LookupPath = "external-system/lookup";

    private readonly IExternalSystemEvidenceLogger _evidenceLogger;
    private readonly ExternalSystemTranslator _translator;

    public ExternalSystemGateway(
        IDapperContext context,
        IHostEnvironment environment,
        HttpClient httpClient,
        IExternalSystemEvidenceLogger evidenceLogger,
        ExternalSystemTranslator translator,
        ILogger<ExternalSystemGateway> logger)
        : base(context, environment, httpClient, logger)
    {
        _evidenceLogger = evidenceLogger;
        _translator = translator;
    }

    public async Task<ExternalLookupResponse> LookupAsync(
        ExternalLookupRequest request,
        CancellationToken cancellationToken)
    {
        var requestPayload = new
        {
            request.RequestId,
            request.SubjectId
        };
        var rawRequestBody = JsonSerializer.Serialize(requestPayload);

        using var message = new HttpRequestMessage(HttpMethod.Post, LookupPath)
        {
            Content = JsonContent.Create(requestPayload)
        };

        message.Headers.Add(CorrelationHeaderName, request.CorrelationId);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await SendAsync(message, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            await _evidenceLogger.CaptureAsync(
                "ExternalSystem.Lookup",
                request.CorrelationId,
                message,
                rawRequestBody,
                response.StatusCode,
                rawBody,
                stopwatch.Elapsed,
                cancellationToken);

            var envelope = ExternalSystemWireEnvelope.Parse(rawBody, response.Content.Headers.ContentType?.MediaType);
            var normalized = _translator.Translate(response.StatusCode, envelope);

            return normalized.Outcome switch
            {
                ExternalSystemOutcome.Success => new ExternalLookupResponse(
                    request.RequestId,
                    normalized.UserName,
                    normalized.BirthDate,
                    ExternalLookupStatus.Ready,
                    normalized.Message),

                ExternalSystemOutcome.NotFound => ExternalLookupResponse.NotFound(
                    request.RequestId,
                    normalized.Message),

                ExternalSystemOutcome.Rejected => ExternalLookupResponse.Rejected(
                    request.RequestId,
                    normalized.Message),

                ExternalSystemOutcome.DependencyFailure => throw new ExternalSystemDependencyException(
                    "External system indicated a dependency failure."),

                _ => throw new ExternalSystemProtocolException(
                    "External system returned contradictory or unclassifiable success/failure signals.")
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            throw ExternalSystemExceptionTranslator.Translate(ex, "ExternalSystem.Lookup");
        }
    }
}
