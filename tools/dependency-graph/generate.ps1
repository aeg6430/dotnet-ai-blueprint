param(
  [string]$Root = (Resolve-Path ".").Path,
  [string]$Out = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Out)) {
  $Out = Join-Path $Root "artifacts/deps.dot"
}

dotnet run --project (Join-Path $PSScriptRoot "DependencyGraph.Tool.csproj") -- --root $Root --out $Out

Write-Host "DOT written to: $Out"
Write-Host "Optional: dot -Tsvg artifacts/deps.dot -o artifacts/deps.svg"

