using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Project.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Map exceptions to appropriate Status Codes and Titles 
            var (statusCode, title) = exception switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                ArgumentException or InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            // Log critical failures for NLog to capture full stack traces
            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
            else
                _logger.LogWarning("Handled Exception: {Title} - {Message}", title, exception.Message);

            httpContext.Response.StatusCode = statusCode;

            // Follow the unified JSON structure defined in ARCHITECTURE.md 
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            }, cancellationToken);

            return true;
        }
    }
}