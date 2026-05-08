using System.IO;
using System.Text;
using NUnit.Framework;

namespace Acme.Tests;

public sealed class PlaceholderGuardTests
{
    [Test]
    public void Placeholders_must_not_remain_in_skeleton()
    {
        // Defensive: prevent "false positive green" when placeholders were not replaced.
        // In the skeleton we check for the most common pack placeholders.

        var root = TestContext.CurrentContext.TestDirectory;
        while (!File.Exists(Path.Combine(root, "StarterPack.Skeleton.sln")) && Directory.GetParent(root) is { } p)
            root = p.FullName;

        Assert.That(File.Exists(Path.Combine(root, "StarterPack.Skeleton.sln")), Is.True, "Could not locate skeleton root (StarterPack.Skeleton.sln).");

        var sb = new StringBuilder();
        foreach (var f in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
        {
            if (f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") ||
                f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            // Do not scan tests; this file contains placeholders by design.
            if (f.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}"))
                continue;

            if (!(f.EndsWith(".cs") || f.EndsWith(".md") || f.EndsWith(".json") || f.EndsWith(".csproj") || f.EndsWith(".sln")))
                continue;

            sb.AppendLine(File.ReadAllText(f));
        }

        var text = sb.ToString();
        Assert.That(text, Does.Not.Contain("{CoreNamespace}"));
        Assert.That(text, Does.Not.Contain("{InfrastructureNamespace}"));
        Assert.That(text, Does.Not.Contain("{ApiNamespace}"));
        Assert.That(text, Does.Not.Contain("{TestsNamespace}"));
    }
}

public sealed class ExceptionLeakTests
{
    [Test]
    public void Db_exceptions_must_not_leak_sensitive_details()
    {
        var problem = Acme.Api.ErrorHandling.ExceptionMapper.ToProblemDetails(new Acme.Api.ErrorHandling.FakeDbException("connection string=..."));
        var json = System.Text.Json.JsonSerializer.Serialize(problem);

        Assert.That(json, Does.Not.Contain("FakeDbException"));
        Assert.That(json, Does.Not.Contain("connection string"));
    }
}

public sealed class DependencyGraphTests
{
    [Test]
    public void Can_emit_dependency_graph_dot()
    {
        var root = TestContext.CurrentContext.TestDirectory;
        while (!File.Exists(Path.Combine(root, "StarterPack.Skeleton.sln")) && Directory.GetParent(root) is { } p)
            root = p.FullName;

        var artifacts = Path.Combine(root, "artifacts");
        Directory.CreateDirectory(artifacts);

        var dotPath = Path.Combine(artifacts, "deps.dot");
        var dot = Acme.Tests.Tools.DependencyGraph.EmitDot(
            ("Acme.Api", new[] { "Acme.Core", "Acme.Infrastructure" }),
            ("Acme.Infrastructure", new[] { "Acme.Core" }),
            ("Acme.Core", Array.Empty<string>()));

        File.WriteAllText(dotPath, dot);
        Assert.That(File.Exists(dotPath), Is.True);
        Assert.That(dot, Does.Contain("digraph"));
    }
}
