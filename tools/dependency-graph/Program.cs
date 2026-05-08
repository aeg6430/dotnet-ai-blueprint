using System.Text;
using System.Xml.Linq;

var root = GetArgValue(args, "--root") ?? Directory.GetCurrentDirectory();
var outPath = GetArgValue(args, "--out");

root = Path.GetFullPath(root);
outPath = outPath is null
    ? Path.Combine(root, "artifacts", "deps.dot")
    : Path.GetFullPath(Path.IsPathRooted(outPath) ? outPath : Path.Combine(root, outPath));

var artifacts = Path.GetDirectoryName(outPath)!;
Directory.CreateDirectory(artifacts);

var projects = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
    .Where(p => !IsIgnoredPath(p))
    .ToList();

var nameByPath = projects.ToDictionary(
    p => p,
    p => Path.GetFileNameWithoutExtension(p),
    StringComparer.OrdinalIgnoreCase);

var edges = new List<(string From, string To)>();

foreach (var proj in projects)
{
    var doc = XDocument.Load(proj);
    var refs = doc.Descendants()
        .Where(e => e.Name.LocalName == "ProjectReference")
        .Select(e => e.Attribute("Include")?.Value)
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .Select(v => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(proj)!, v!)))
        .ToList();

    var fromName = nameByPath[proj];
    foreach (var refPath in refs)
    {
        if (!nameByPath.TryGetValue(refPath, out var toName))
        {
            // Reference may point outside root; keep a stable node name.
            toName = Path.GetFileNameWithoutExtension(refPath);
        }

        edges.Add((fromName, toName));
    }
}

var dot = EmitDot(edges);
File.WriteAllText(outPath, dot, Encoding.UTF8);

Console.WriteLine($"Wrote: {outPath}");
Console.WriteLine($"Projects: {projects.Count}, Edges: {edges.Count}");

static string EmitDot(List<(string From, string To)> edges)
{
    var sb = new StringBuilder();
    sb.AppendLine("digraph deps {");
    sb.AppendLine("  rankdir=LR;");

    foreach (var (from, to) in edges.Distinct())
        sb.AppendLine(FormattableString.Invariant($"  \"{from}\" -> \"{to}\";"));

    sb.AppendLine("}");
    return sb.ToString();
}

static bool IsIgnoredPath(string path)
{
    var sep = Path.DirectorySeparatorChar;
    return path.Contains($"{sep}bin{sep}", StringComparison.OrdinalIgnoreCase) ||
           path.Contains($"{sep}obj{sep}", StringComparison.OrdinalIgnoreCase);
}

static string? GetArgValue(string[] args, string key)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (!string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            continue;
        if (i + 1 >= args.Length)
            return null;
        return args[i + 1];
    }

    return null;
}

