# `_starter-pack-seed/` (export)

This folder contains materials for a **minimal starter pack working directory** you can copy into another repository and use for project setup.

Per original pack policy:

- Do **not** commit anything automatically from generated working directories.
- Do **not** change `.gitignore` from here.
- This repository remains the source of truth; export is a **plain file copy**.

## What you get

When the working directory content is prepared, `_starter-pack-seed/out/` contains the portable starter-pack files copied from the **parent of this folder** (the repository root):

- `.cursor/rules/**` (supported Cursor entrypoint / read-order index)
- `docs/starter-pack/**`
- `.github/copilot-instructions.md`
- `templates/**` (includes optional Serilog swap-ins: `Program.Serilog.cs`, `appsettings.serilog.json` — see [`docs/starter-pack/optional/logging/serilog.md`](../docs/starter-pack/optional/logging/serilog.md))
- `docs/rules/**`
- `docs/ARCHITECTURE.md`
- `docs/adr/template.md` (ADR template)

## Preferred setup flow

If you already have the exported folder, treat it as a normal working directory rather than a special install artifact.

1. Open the working directory in Cursor or your IDE.
2. Ask AI to read `docs/starter-pack/project-setup-protocol.md` and follow the `Project Setup Protocol`.
3. Provide `TargetProjectName` as the project setup value.
4. Let AI rename namespaces, project names, solution names, and file paths that still reflect the current starter-pack identity.
5. Complete naming and path adjustments inside this working directory before moving content into the destination project.

## Notes

- The export is intentionally a plain file copy (no git operations).
- The exported content is still just a normal working directory; it can be used directly with the preferred setup flow above.
- If your team materializes the working directory under `_starter-pack-seed/out/` and runs Apex One or similar endpoint protection, discuss whether that path should be treated as a reviewed exclusion because large rename/copy operations can trigger heavy real-time scanning.
- `.cursor/rules/` and `.github/copilot-instructions.md` are the supported AI entrypoints in the exported seed; the shared setup flow lives in `docs/starter-pack/project-setup-protocol.md`.
- `docs/ARCHITECTURE.md`, `docs/rules/**`, `templates/`, and `skeleton/` remain the source of truth for the exported pack.
- External URLs in markdown are kept as-is.
