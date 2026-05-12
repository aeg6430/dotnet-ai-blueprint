namespace StarterPack.Setup;

internal sealed class SetupWorkspace
{
    internal SetupWorkspace(string root)
    {
        Root = Path.GetFullPath(root);
        if (!Directory.Exists(Root))
            throw new DirectoryNotFoundException($"Root directory not found: {Root}");
    }

    internal string Root { get; }

    internal IEnumerable<string> EnumerateEligibleFiles() =>
        Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(path =>
                SetupConventions.EligibleExtensions.Contains(Path.GetExtension(path)) &&
                !IsIgnoredPath(path));

    internal IEnumerable<string> EnumerateRenameDirectories() =>
        Directory.EnumerateDirectories(Root, "*", SearchOption.AllDirectories)
            .Where(path => !IsIgnoredPath(path))
            .OrderByDescending(path => path.Length);

    internal IEnumerable<string> EnumerateRenameFiles(Func<string, bool>? includePredicate = null) =>
        Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(path => !IsIgnoredPath(path))
            .Where(path => includePredicate?.Invoke(path) ?? true)
            .OrderByDescending(path => path.Length);

    internal IEnumerable<string> EnumerateGeneratedDirectories() =>
        Directory.EnumerateDirectories(Root, "*", SearchOption.AllDirectories)
            .Where(path =>
                !string.Equals(Path.GetFullPath(path), Root, StringComparison.OrdinalIgnoreCase) &&
                SetupConventions.GeneratedDirectoryNames.Contains(Path.GetFileName(path)))
            .OrderByDescending(path => path.Length);

    internal string GetRelativePath(string path) => Path.GetRelativePath(Root, path);

    internal bool IsIgnoredPath(string path)
    {
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var parts = normalized.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        return parts.Any(SetupConventions.IgnoredDirectoryNames.Contains);
    }

    internal bool IsUnderTestsDirectory(string path)
    {
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var parts = normalized.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        return parts.Any(part => string.Equals(part, "tests", StringComparison.OrdinalIgnoreCase));
    }

    internal static string ApplyReplacements(string input, IReadOnlyList<KeyValuePair<string, string>> replacements)
    {
        var updated = input;
        foreach (var replacement in replacements.OrderByDescending(r => r.Key.Length))
        {
            updated = updated.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        return updated;
    }

    internal static int CountOccurrences(string text, string marker)
    {
        var count = 0;
        var start = 0;

        while (start < text.Length)
        {
            var index = text.IndexOf(marker, start, StringComparison.Ordinal);
            if (index < 0)
                break;

            count++;
            start = index + marker.Length;
        }

        return count;
    }
}
