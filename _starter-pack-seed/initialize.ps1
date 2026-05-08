param(
  [Parameter(Mandatory = $true)]
  [string]$Solution,

  [Parameter(Mandatory = $true)]
  [string]$CoreNamespace,

  [Parameter(Mandatory = $true)]
  [string]$InfrastructureNamespace,

  [Parameter(Mandatory = $true)]
  [string]$ApiNamespace,

  [Parameter(Mandatory = $true)]
  [string]$TestsNamespace,

  [string]$Root = (Resolve-Path ".").Path
)

$ErrorActionPreference = "Stop"

Write-Host "Initializing starter pack placeholders under: $Root"

$replacements = @(
  @{ Token = "{Solution}"; Value = $Solution },
  @{ Token = "{CoreNamespace}"; Value = $CoreNamespace },
  @{ Token = "{InfrastructureNamespace}"; Value = $InfrastructureNamespace },
  @{ Token = "{ApiNamespace}"; Value = $ApiNamespace },
  @{ Token = "{TestsNamespace}"; Value = $TestsNamespace }
)

$files = Get-ChildItem -LiteralPath $Root -Recurse -File |
  Where-Object {
    $_.FullName -notmatch '\\bin\\' -and
    $_.FullName -notmatch '\\obj\\' -and
    ($_.Name -like "*.cs" -or $_.Name -like "*.md" -or $_.Name -like "*.csproj" -or $_.Name -like "*.sln" -or $_.Name -like "*.json")
  }

foreach ($f in $files) {
  $text = Get-Content -LiteralPath $f.FullName -Raw
  $updated = $text
  foreach ($r in $replacements) {
    $updated = $updated.Replace($r.Token, $r.Value)
  }

  if ($updated -ne $text) {
    Set-Content -LiteralPath $f.FullName -Value $updated -Encoding UTF8
  }
}

Write-Host "Done. Next: run your test suite to ensure placeholder guards are green."

