param(
  [string]$RepoRoot = (Resolve-Path ".").Path,
  [string]$OutDir = ""
)

$ErrorActionPreference = "Stop"

$PackRoot = Split-Path -Parent $PSScriptRoot

function Ensure-Dir([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Path $Path | Out-Null
  }
}

function Copy-Tree([string]$Source, [string]$Dest) {
  if (-not (Test-Path -LiteralPath $Source)) {
    throw "Missing source path: $Source"
  }
  Ensure-Dir $Dest

  # Use robocopy when available for speed + predictable directory copy.
  $robo = Get-Command "robocopy" -ErrorAction SilentlyContinue
  if ($null -ne $robo) {
    # /MIR is intentionally NOT used; we only want to copy current files into a fresh out dir.
    # /NFL /NDL reduce noise, /NJH /NJS hide headers, /NP no progress.
    $args = @($Source, $Dest, "/E", "/NFL", "/NDL", "/NJH", "/NJS", "/NP")
    & robocopy @args | Out-Null
    # robocopy uses bitmask exit codes; 0..7 are success.
    if ($LASTEXITCODE -gt 7) {
      throw "robocopy failed with exit code $LASTEXITCODE for $Source -> $Dest"
    }
    return
  }

  Copy-Item -Path (Join-Path $Source "*") -Destination $Dest -Recurse -Force
}

function Copy-File([string]$Source, [string]$Dest) {
  if (-not (Test-Path -LiteralPath $Source)) {
    throw "Missing source file: $Source"
  }
  Ensure-Dir (Split-Path -Parent $Dest)
  Copy-Item -LiteralPath $Source -Destination $Dest -Force
}

if ([string]::IsNullOrWhiteSpace($OutDir)) {
  $OutDir = Join-Path $RepoRoot "_starter-pack-seed/out"
}

Write-Host "RepoRoot: $RepoRoot"
Write-Host "PackRoot: $PackRoot"
Write-Host "OutDir:   $OutDir"

# Start clean (only under _starter-pack-seed/out)
if (Test-Path -LiteralPath $OutDir) {
  Remove-Item -LiteralPath $OutDir -Recurse -Force
}
Ensure-Dir $OutDir

# 1) Starter pack tree
Copy-Tree (Join-Path $PackRoot "docs/starter-pack") (Join-Path $OutDir "docs/starter-pack")

# 2) Copilot entrypoint
Copy-File (Join-Path $PackRoot ".github/copilot-instructions.md") (Join-Path $OutDir ".github/copilot-instructions.md")

# 3) Templates (sample patterns)
Copy-Tree (Join-Path $PackRoot "templates") (Join-Path $OutDir "templates")

# 4) Binding rules (rules)
Copy-Tree (Join-Path $PackRoot "docs/rules") (Join-Path $OutDir "docs/rules")
Copy-File (Join-Path $PackRoot "docs/ARCHITECTURE.md") (Join-Path $OutDir "docs/ARCHITECTURE.md")

# 5) ADR template (optional but useful)
Copy-File (Join-Path $PackRoot "docs/adr/template.md") (Join-Path $OutDir "docs/adr/template.md")

# 5.1) Tools (optional)
if (Test-Path -LiteralPath (Join-Path $PackRoot "tools")) {
  Copy-Tree (Join-Path $PackRoot "tools") (Join-Path $OutDir "tools")
}

# 6) Minimal top-level README for the exported seed
$exportReadme = @"
# Layered .NET Starter (export)

This folder was exported from another repository using `_starter-pack-seed/build-seed.ps1`.

Start here:

- `docs/starter-pack/README.md`
- `.github/copilot-instructions.md`

Then copy the `*.cs.txt` templates into your solution, replacing placeholders:
`{Solution}`, `{CoreNamespace}`, `{InfrastructureNamespace}`, `{ApiNamespace}`, `{TestsNamespace}`.

Recommended (safer): run the initializer to replace placeholders in one go:
`pwsh -File ./initialize.ps1 -Solution "<Solution>" -CoreNamespace "<Core>" -InfrastructureNamespace "<Infra>" -ApiNamespace "<Api>" -TestsNamespace "<Tests>"`

Optional tool:
- `tools/dependency-graph/` (emit Graphviz DOT from csproj references)
"@

$exportReadmePath = Join-Path $OutDir "README.md"
Set-Content -Path $exportReadmePath -Value $exportReadme -Encoding UTF8

# 7) Placeholder initializer (optional but recommended)
Copy-File (Join-Path $PSScriptRoot "initialize.ps1") (Join-Path $OutDir "initialize.ps1")

Write-Host "Done. Export created at: $OutDir"

