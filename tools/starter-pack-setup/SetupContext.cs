namespace StarterPack.Setup;

internal sealed record SetupContext(
    string TargetProjectName,
    string SolutionName,
    string CoreNamespace,
    string InfrastructureNamespace,
    string ApiNamespace,
    string TestsNamespace);

internal sealed record MarkerHit(string Path, string Marker, int Count);

internal sealed record ScanMarker(string Value, bool SkipTestsDirectory = false);
