# Project Setup Protocol

Use this protocol when a Seed folder, exported starter pack, or working directory must be renamed and aligned to a real target project **before** implementation work begins.

This document is **tool-neutral**. It is the canonical setup path for Cursor, GitHub Copilot, and restricted environments where only plain chat or direct editing is available.

> Note
> The three-directory workflow remains understandable by following this document directly. When automation is involved, prefer stable `Makefile` targets over ad hoc command synthesis or the removed legacy `.ps1` flows. The goal is to keep setup understandable while giving automated workflows a consistent command surface that reduces endpoint-protection friction and unintended code changes.

## When to use it

Use this protocol for:

- namespace conversion
- project / solution renaming
- folder path alignment
- placeholder cleanup tied to project identity
- preparing a Seed / working directory before translating logic into a Target repo

## Read first

Before running setup, read:

1. [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md)
2. [`docs/rules/architecture-protocol.md`](../rules/architecture-protocol.md)
3. If the target repo provides `docs/specs/`, read the relevant spec first

If the task involves performance constraints, build/test slowness, endpoint exclusions, or restricted cloud tooling, also read [`docs/rules/endpoint-protection.md`](../rules/endpoint-protection.md).

## Required inputs

- `TargetProjectName` (or the target repo's approved naming scheme)
- the current Seed / working directory contents
- any target naming rules defined in `docs/specs/` or the real product repository

Treat `TargetProjectName` as a **user-provided setup value**, not as a hard-coded repo name.
If setup is exposed through helper automation such as a `Makefile`, require `TargetProjectName` as an explicit argument or variable. Do not hide it behind a repo-default constant, and do not assume an interactive prompt is always available.

## Canonical prompt

```text
Read `docs/starter-pack/project-setup-protocol.md`.
In this directory, convert namespaces, project names, solution names, directory paths, and setup-related references from the current Blueprint / Seed shape to `TargetProjectName`.
Keep the architecture, rule intent, and folder responsibilities intact.
If `docs/specs/` or the target repo defines a different naming scheme, follow that instead of the default.
After setup, summarize any unresolved naming conflicts or review items.
```

## Protocol

1. Read the architecture and rule entry docs listed above.
2. Identify project-identity markers that still reflect the Blueprint / Seed shape:
   - namespaces
   - project names
   - solution names
   - directory names
   - setup references in docs or config
3. Rename those markers to match `TargetProjectName` or the target repo's approved naming scheme.
4. Preserve reusable rules, templates, and architecture boundaries unless the target project explicitly changes them.
5. Prefer one consistent naming pass rather than mixing partial direct replacements with partial automated replacements.
6. After setup, continue with target-aware implementation:
   - existing / legacy target repo: preserve target naming, style, and boundaries
   - new / early-stage target repo: remove leftover Blueprint identity and keep the structure target-native

## Fallback when assisted tooling is limited

If Cursor cloud features, Copilot agents, or similar assisted modes are unavailable:

- use plain chat or direct editing with the same protocol, or invoke reviewed `Makefile` targets when available
- prefer split setup targets such as scan, placeholder rewrite, content rewrite, solution/project rename, path rename, cleanup, and verify over a single opaque script step
- apply setup in small reviewable batches (namespaces, project files, folders, config)
- validate after each batch rather than attempting one opaque bulk rewrite

## Endpoint-protection note

Large rename/move operations and test runs can trigger heavy real-time scanning in environments such as Apex One.

- For high-churn working folders or Build Agent workspaces, coordinate reviewed exclusion candidates using [`docs/rules/endpoint-protection.md`](../rules/endpoint-protection.md).
- If the environment is heavily restricted, prefer smaller setup batches and static/source-oriented validation before heavier reflection-based checks.
