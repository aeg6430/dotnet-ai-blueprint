using System.Text;

namespace StarterPack.Setup;

internal sealed class SetupRewriter
{
    private readonly SetupWorkspace _workspace;

    internal SetupRewriter(SetupWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal void RewritePlaceholders(SetupContext context, bool dryRun) =>
        RewriteFiles(
            SetupConventions.BuildPlaceholderReplacements(context),
            dryRun,
            "placeholder",
            skipPredicate: _workspace.IsUnderTestsDirectory);

    internal void RewriteContent(SetupContext context, bool dryRun) =>
        RewriteFiles(SetupConventions.BuildContentReplacements(context), dryRun, "content");

    private void RewriteFiles(
        IReadOnlyList<KeyValuePair<string, string>> replacements,
        bool dryRun,
        string label,
        Func<string, bool>? skipPredicate = null)
    {
        var changedFiles = 0;

        foreach (var file in _workspace.EnumerateEligibleFiles())
        {
            if (skipPredicate?.Invoke(file) == true)
                continue;

            var original = File.ReadAllText(file);
            var updated = SetupWorkspace.ApplyReplacements(original, replacements);
            if (string.Equals(original, updated, StringComparison.Ordinal))
                continue;

            changedFiles++;
            Console.WriteLine($"{(dryRun ? "Would update" : "Updated")}: {_workspace.GetRelativePath(file)}");

            if (!dryRun)
                File.WriteAllText(file, updated, new UTF8Encoding(false));
        }

        Console.WriteLine($"{label} rewrite {(dryRun ? "previewed" : "completed")}. Files changed: {changedFiles}");
    }
}
