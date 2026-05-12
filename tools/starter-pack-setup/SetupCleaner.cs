namespace StarterPack.Setup;

internal sealed class SetupCleaner
{
    private readonly SetupWorkspace _workspace;

    internal SetupCleaner(SetupWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal void CleanupGeneratedDirectories(bool dryRun)
    {
        var candidates = _workspace.EnumerateGeneratedDirectories().ToList();

        foreach (var directory in candidates)
        {
            Console.WriteLine($"{(dryRun ? "Would remove" : "Removed")}: {_workspace.GetRelativePath(directory)}");

            if (!dryRun)
                Directory.Delete(directory, recursive: true);
        }

        Console.WriteLine($"Cleanup {(dryRun ? "previewed" : "completed")}. Directories removed: {candidates.Count}");
    }
}
