# `_starter-pack-seed/` (export)

This folder helps you **export a minimal starter pack** you can copy into another repository.

Per original pack policy:

- Do **not** commit anything automatically from these scripts.
- Do **not** change `.gitignore` from here.
- This repository remains the source of truth; export is a **plain file copy**.

## What you get

Running `build-seed.ps1` creates:

`_starter-pack-seed/out/`

Containing (copied from the **parent of this folder**, i.e. repository root):

- `.cursor/rules/**` (supported Cursor entrypoint / read-order index)
- `docs/starter-pack/**`
- `.github/copilot-instructions.md`
- `templates/**` (includes optional Serilog swap-ins: `Program.Serilog.cs`, `appsettings.serilog.json` — see [`docs/starter-pack/optional/logging/serilog.md`](../docs/starter-pack/optional/logging/serilog.md))
- `docs/rules/**`
- `docs/ARCHITECTURE.md`
- `docs/adr/template.md` (ADR template)

## How to run (PowerShell)

`build-seed.ps1` reads pack content from the parent directory (`..` = repository root) and writes to `$RepoRoot/_starter-pack-seed/out` by default (`$RepoRoot` defaults to the current directory when you invoke the script).

From **this repository root**:

```powershell
pwsh -NoProfile -File "_starter-pack-seed/build-seed.ps1"
```

Optional: set output directory explicitly:

```powershell
pwsh -NoProfile -File "_starter-pack-seed/build-seed.ps1" -RepoRoot "." -OutDir ".\_starter-pack-seed\out"
```

Then **copy** `_starter-pack-seed/out/` (or your `-OutDir`) into your target repository root.

## Notes

- The export is intentionally a plain file copy (no git operations).
- `.cursor/rules/` is the stable, versioned, and only supported Cursor entrypoint in the exported seed.
- `docs/ARCHITECTURE.md`, `docs/rules/**`, `templates/`, and `skeleton/` remain the source of truth for the exported pack.
- External URLs in markdown are kept as-is.
