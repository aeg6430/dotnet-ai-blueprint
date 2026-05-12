using System;
using Microsoft.AspNetCore.Http;

namespace Project.Api.Middlewares;

public sealed class RequestScreeningOptions
{
    public const string SectionName = "RequestScreening";

    public bool Enabled { get; set; }

    public bool ReportOnly { get; set; }

    public int BlockStatusCode { get; set; } = StatusCodes.Status403Forbidden;

    public string ProblemType { get; set; } = "https://httpstatuses.com/403";

    public string ProblemTitle { get; set; } = "Request blocked";

    public string ProblemDetail { get; set; } =
        "This request has been blocked by a temporary security control.";

    public string[] BlockedPathPrefixes { get; set; } = Array.Empty<string>();

    public string[] BlockedMethods { get; set; } = Array.Empty<string>();

    public string[] BlockedQueryKeys { get; set; } = Array.Empty<string>();

    public string[] BlockedQueryValueFragments { get; set; } = Array.Empty<string>();

    public string[] AllowlistedPathPrefixes { get; set; } = Array.Empty<string>();
}
