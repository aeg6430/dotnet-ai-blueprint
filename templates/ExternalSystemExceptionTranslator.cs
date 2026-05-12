using System.Text.Json;
using System.Xml;
using Polly.Timeout;

namespace Project.Infrastructure.Adapters;

// TEMPLATE — normalize foreign/runtime/protocol faults before they escape the adapter boundary.
// Replace these exception types with your project's approved dependency/protocol exception hierarchy if needed.
public sealed class ExternalSystemDependencyException : Exception
{
    public ExternalSystemDependencyException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ExternalSystemProtocolException : Exception
{
    public ExternalSystemProtocolException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public static class ExternalSystemExceptionTranslator
{
    public static Exception Translate(Exception exception, string operationName)
    {
        return exception switch
        {
            ExternalSystemDependencyException or ExternalSystemProtocolException => exception,

            TimeoutRejectedException => new ExternalSystemDependencyException(
                $"External dependency timed out during {operationName}.",
                exception),

            TaskCanceledException => new ExternalSystemDependencyException(
                $"External dependency timed out or was canceled during {operationName}.",
                exception),

            HttpRequestException => new ExternalSystemDependencyException(
                $"External dependency request failed during {operationName}.",
                exception),

            JsonException or XmlException or FormatException => new ExternalSystemProtocolException(
                $"External dependency returned a malformed or unsupported payload during {operationName}.",
                exception),

            _ => new ExternalSystemDependencyException(
                $"External dependency failed during {operationName}.",
                exception)
        };
    }
}
