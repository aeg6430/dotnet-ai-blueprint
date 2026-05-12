using StarterPack.Setup;

var command = args.FirstOrDefault();
if (string.IsNullOrWhiteSpace(command) || IsHelp(command))
{
    PrintHelp();
    return;
}

var root = GetArgValue(args, "--root") ?? Directory.GetCurrentDirectory();
var dryRun = HasArg(args, "--dry-run");

SetupWorkspace workspace;
try
{
    workspace = new SetupWorkspace(root);
}
catch (Exception ex)
{
    Fail(ex.Message);
    return;
}

var scanner = new SetupScanner(workspace);
var rewriter = new SetupRewriter(workspace);
var pathRenamer = new SetupPathRenamer(workspace);
var cleaner = new SetupCleaner(workspace);

try
{
    switch (command)
    {
        case "scan":
            scanner.Scan(failOnHits: false);
            break;

        case "verify":
            Environment.ExitCode = scanner.Scan(failOnHits: true);
            break;

        case "rewrite-placeholders":
            rewriter.RewritePlaceholders(BuildSetupContext(args), dryRun);
            break;

        case "rewrite-content":
            rewriter.RewriteContent(BuildSetupContext(args), dryRun);
            break;

        case "rename-solution-projects":
            pathRenamer.RenameSolutionAndProjectFiles(BuildSetupContext(args), dryRun);
            break;

        case "rename-paths":
            pathRenamer.RenameRemainingPaths(BuildSetupContext(args), dryRun);
            break;

        case "cleanup":
            cleaner.CleanupGeneratedDirectories(dryRun);
            break;

        default:
            Fail($"Unknown command '{command}'.");
            PrintHelp();
            break;
    }
}
catch (Exception ex)
{
    Fail(ex.Message);
}

SetupContext BuildSetupContext(string[] currentArgs)
{
    var targetProjectName = RequireArg(currentArgs, "--target-project-name");

    return new SetupContext(
        targetProjectName,
        GetArgValue(currentArgs, "--solution-name") ?? targetProjectName,
        GetArgValue(currentArgs, "--core-namespace") ?? $"{targetProjectName}.Core",
        GetArgValue(currentArgs, "--infrastructure-namespace") ?? $"{targetProjectName}.Infrastructure",
        GetArgValue(currentArgs, "--api-namespace") ?? $"{targetProjectName}.Api",
        GetArgValue(currentArgs, "--tests-namespace") ?? $"{targetProjectName}.Tests");
}

string RequireArg(string[] currentArgs, string key)
{
    var value = GetArgValue(currentArgs, key);
    if (!string.IsNullOrWhiteSpace(value))
        return value;

    throw new InvalidOperationException($"Missing required argument: {key}");
}

string? GetArgValue(string[] currentArgs, string key)
{
    for (var i = 0; i < currentArgs.Length; i++)
    {
        if (!string.Equals(currentArgs[i], key, StringComparison.OrdinalIgnoreCase))
            continue;

        if (i + 1 >= currentArgs.Length)
            return null;

        return currentArgs[i + 1];
    }

    return null;
}

bool HasArg(string[] currentArgs, string key) =>
    currentArgs.Any(arg => string.Equals(arg, key, StringComparison.OrdinalIgnoreCase));

bool IsHelp(string currentCommand) =>
    string.Equals(currentCommand, "--help", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(currentCommand, "-h", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(currentCommand, "help", StringComparison.OrdinalIgnoreCase);

void PrintHelp()
{
    Console.WriteLine("Starter Pack Setup Tool");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  scan");
    Console.WriteLine("  verify");
    Console.WriteLine("  rewrite-placeholders --target-project-name <Name>");
    Console.WriteLine("  rewrite-content --target-project-name <Name>");
    Console.WriteLine("  rename-solution-projects --target-project-name <Name>");
    Console.WriteLine("  rename-paths --target-project-name <Name>");
    Console.WriteLine("  cleanup");
    Console.WriteLine();
    Console.WriteLine("Optional arguments:");
    Console.WriteLine("  --root <dir>");
    Console.WriteLine("  --solution-name <Name>");
    Console.WriteLine("  --core-namespace <Name>");
    Console.WriteLine("  --infrastructure-namespace <Name>");
    Console.WriteLine("  --api-namespace <Name>");
    Console.WriteLine("  --tests-namespace <Name>");
    Console.WriteLine("  --dry-run");
}

void Fail(string message)
{
    Console.Error.WriteLine(message);
    Environment.ExitCode = 1;
}
