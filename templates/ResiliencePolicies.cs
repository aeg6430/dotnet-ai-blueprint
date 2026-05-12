using System.Net;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — requires Polly + Polly.Extensions.Http packages in the host project.
public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                2,
                retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
}
