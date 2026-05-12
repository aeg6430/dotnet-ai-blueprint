namespace StarterPack.Setup;

internal static class SetupConventions
{
    internal static readonly HashSet<string> EligibleExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".csproj",
        ".http",
        ".json",
        ".md",
        ".sln",
    };

    internal static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".idea",
        ".vs",
        "artifacts",
        "bin",
        "coverage",
        "obj",
        "out",
        "publish",
        "TestResults",
    };

    internal static readonly HashSet<string> GeneratedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "artifacts",
        "bin",
        "coverage",
        "obj",
        "out",
        "publish",
        "TestResults",
    };

    internal static IReadOnlyList<KeyValuePair<string, string>> BuildPlaceholderReplacements(SetupContext context) =>
        new List<KeyValuePair<string, string>>
        {
            new("{Solution}", context.SolutionName),
            new("{CoreNamespace}", context.CoreNamespace),
            new("{InfrastructureNamespace}", context.InfrastructureNamespace),
            new("{ApiNamespace}", context.ApiNamespace),
            new("{TestsNamespace}", context.TestsNamespace),
        };

    internal static IReadOnlyList<KeyValuePair<string, string>> BuildContentReplacements(SetupContext context) =>
        new List<KeyValuePair<string, string>>
        {
            new("StarterPack.Skeleton", context.SolutionName),
            new("Acme.Infrastructure", context.InfrastructureNamespace),
            new("Acme.Tests", context.TestsNamespace),
            new("Acme.Core", context.CoreNamespace),
            new("Acme.Api", context.ApiNamespace),
        };

    internal static IReadOnlyList<ScanMarker> BuildScanMarkers() =>
        new List<ScanMarker>
        {
            new("{Solution}", SkipTestsDirectory: true),
            new("{CoreNamespace}", SkipTestsDirectory: true),
            new("{InfrastructureNamespace}", SkipTestsDirectory: true),
            new("{ApiNamespace}", SkipTestsDirectory: true),
            new("{TestsNamespace}", SkipTestsDirectory: true),
            new("StarterPack.Skeleton"),
            new("Acme.Infrastructure"),
            new("Acme.Tests"),
            new("Acme.Core"),
            new("Acme.Api"),
        };
}
