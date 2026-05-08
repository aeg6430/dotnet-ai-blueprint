using System.Text;

namespace Acme.Tests.Tools;

public static class DependencyGraph
{
    public static string EmitDot(params (string Node, string[] DependsOn)[] edges)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph deps {");

        foreach (var (node, deps) in edges)
        {
            if (deps.Length == 0)
            {
                sb.AppendLine($"  \"{node}\";");
                continue;
            }

            foreach (var dep in deps)
                sb.AppendLine($"  \"{node}\" -> \"{dep}\";");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}

