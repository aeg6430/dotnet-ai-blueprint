namespace StarterPack.Setup;

internal sealed class SetupPathRenamer
{
    private readonly SetupWorkspace _workspace;

    internal SetupPathRenamer(SetupWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal void RenameSolutionAndProjectFiles(SetupContext context, bool dryRun)
    {
        var replacements = SetupConventions.BuildContentReplacements(context);
        RenameFiles(
            replacements,
            dryRun,
            path =>
            {
                var extension = Path.GetExtension(path);
                return string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase);
            },
            "solution/project rename");
    }

    internal void RenameRemainingPaths(SetupContext context, bool dryRun)
    {
        var replacements = SetupConventions.BuildContentReplacements(context);
        var renamedDirectories = RenameDirectories(replacements, dryRun);
        var renamedFiles = RenameFiles(
            replacements,
            dryRun,
            path =>
            {
                var extension = Path.GetExtension(path);
                return !string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase);
            },
            "path rename",
            suppressSummary: true);

        Console.WriteLine(
            $"Path rename {(dryRun ? "previewed" : "completed")}. Files: {renamedFiles}, Directories: {renamedDirectories}");
    }

    private int RenameDirectories(IReadOnlyList<KeyValuePair<string, string>> replacements, bool dryRun)
    {
        var renamedDirectories = 0;

        foreach (var directory in _workspace.EnumerateRenameDirectories())
        {
            var name = Path.GetFileName(directory);
            var updatedName = SetupWorkspace.ApplyReplacements(name, replacements);
            if (string.Equals(name, updatedName, StringComparison.Ordinal))
                continue;

            var parent = Path.GetDirectoryName(directory);
            if (string.IsNullOrWhiteSpace(parent))
                continue;

            var destination = Path.Combine(parent, updatedName);
            Console.WriteLine(
                $"{(dryRun ? "Would rename" : "Renamed")}: {_workspace.GetRelativePath(directory)} -> {_workspace.GetRelativePath(destination)}");

            if (!dryRun)
            {
                if (Directory.Exists(destination))
                    throw new InvalidOperationException(
                        $"Cannot rename '{_workspace.GetRelativePath(directory)}' because '{_workspace.GetRelativePath(destination)}' already exists.");

                Directory.Move(directory, destination);
            }

            renamedDirectories++;
        }

        return renamedDirectories;
    }

    private int RenameFiles(
        IReadOnlyList<KeyValuePair<string, string>> replacements,
        bool dryRun,
        Func<string, bool> includePredicate,
        string summaryLabel,
        bool suppressSummary = false)
    {
        var renamedFiles = 0;

        foreach (var file in _workspace.EnumerateRenameFiles(includePredicate))
        {
            var name = Path.GetFileName(file);
            var updatedName = SetupWorkspace.ApplyReplacements(name, replacements);
            if (string.Equals(name, updatedName, StringComparison.Ordinal))
                continue;

            var parent = Path.GetDirectoryName(file);
            if (string.IsNullOrWhiteSpace(parent))
                continue;

            var destination = Path.Combine(parent, updatedName);
            Console.WriteLine(
                $"{(dryRun ? "Would rename" : "Renamed")}: {_workspace.GetRelativePath(file)} -> {_workspace.GetRelativePath(destination)}");

            if (!dryRun)
            {
                if (File.Exists(destination))
                    throw new InvalidOperationException(
                        $"Cannot rename '{_workspace.GetRelativePath(file)}' because '{_workspace.GetRelativePath(destination)}' already exists.");

                File.Move(file, destination);
            }

            renamedFiles++;
        }

        if (!suppressSummary)
            Console.WriteLine($"{summaryLabel} {(dryRun ? "previewed" : "completed")}. Files: {renamedFiles}");

        return renamedFiles;
    }
}
