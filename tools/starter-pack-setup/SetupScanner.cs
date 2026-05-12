namespace StarterPack.Setup;

internal sealed class SetupScanner
{
    private readonly SetupWorkspace _workspace;

    internal SetupScanner(SetupWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal int Scan(bool failOnHits)
    {
        var markers = SetupConventions.BuildScanMarkers();
        var hits = new List<MarkerHit>();

        foreach (var file in _workspace.EnumerateEligibleFiles())
        {
            var text = File.ReadAllText(file);
            foreach (var marker in markers)
            {
                if (marker.SkipTestsDirectory && _workspace.IsUnderTestsDirectory(file))
                    continue;

                var count = SetupWorkspace.CountOccurrences(text, marker.Value);
                if (count == 0)
                    continue;

                hits.Add(new MarkerHit(_workspace.GetRelativePath(file), marker.Value, count));
            }
        }

        Console.WriteLine($"Scanning: {_workspace.Root}");

        if (hits.Count == 0)
        {
            Console.WriteLine("No setup markers found.");
            return 0;
        }

        Console.WriteLine("Found setup markers:");
        foreach (var hit in hits
                     .OrderBy(h => h.Path, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(h => h.Marker, StringComparer.Ordinal))
        {
            Console.WriteLine($"- {hit.Path}: {hit.Marker} ({hit.Count})");
        }

        if (failOnHits)
        {
            Console.Error.WriteLine("Verification failed: setup markers still remain.");
            return 1;
        }

        return 0;
    }
}
