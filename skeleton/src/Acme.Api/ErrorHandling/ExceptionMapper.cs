using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace Acme.Api.ErrorHandling;

public static class ExceptionMapper
{
    // This is intentionally minimal. In real projects, wire this into the global exception boundary.
    public static ProblemDetails ToProblemDetails(Exception exception)
    {
        if (exception is DbException)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "Internal Server Error"
            };
        }

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "Internal Server Error"
        };
    }
}

public sealed class FakeDbException(string message) : DbException(message)
{
}

