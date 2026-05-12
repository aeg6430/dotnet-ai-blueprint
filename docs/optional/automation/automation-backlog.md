---
inclusion: manual
---

# Automation Backlog (Backend)

This backlog lists **rules that are not yet fully automated** (or are only partially automated) and proposes the next enforcement steps.

The goal is to turn high-signal rules into **repeatable gates** without creating noisy false positives.

Path placeholders such as `{BackendRoot}` and `{CorePrj}` match [automation-coverage.md](automation-coverage.md).

Scoring rubric: see [automation-decision-matrix.md](automation-decision-matrix.md).

## P0 — Documentation integrity (keep the map trustworthy)

- **AB-0001: Align docs with actual analyzer config**
  - **Why**: [automation-coverage.md](automation-coverage.md) and the analyzer baseline narrative must reflect what `.editorconfig` enforces today.
  - **Mechanism**: docs-only update (no code changes).
  - **Scope**: this folder’s `automation-coverage.md` + `automation-decision-matrix.md` (if needed).

- **AB-0002: Retire the legacy `.cursorrules` bridge after downstream migration**
  - **Why**: `.cursor/rules/*.mdc` is now the primary Cursor mechanism, but the root bridge remains for compatibility.
  - **Decision**: keep the bridge while downstream repos and docs still rely on it, then remove it once `.cursor/rules/` is universally adopted.
  - **Mechanism**: docs-only update.
  - **Exit criteria**:
    1. `_starter-pack-seed/build-seed.ps1` and exported onboarding material start from `.cursor/rules/`, not `.cursorrules`.
    2. Repository/starter-pack docs describe `.cursorrules` as compatibility-only and do not require it for any supported setup path.
    3. Supported downstream repos, templates, or internal bootstrap docs no longer have an active dependency on the root `.cursorrules` file.
    4. The retirement change removes `.cursorrules` from the repo root, export path, and remaining doc references in the same cleanup pass.
  - **Next actions**:
    - Audit downstream starter repos/templates for hard references to `.cursorrules`.
    - Track which consumers still need the bridge, with an owner and removal target date.
    - Remove the bridge only after the audit is clear or the remaining consumers are explicitly out of support.

## P1 — High-signal code-quality via analyzers (scoped, then promote)

- **AB-0101: `nameof()` enforcement where applicable**
  - **Rule source**: `.cursor/rules/*.mdc` (prefer `nameof()`).
  - **Mechanism**: analyzer selection (likely NetAnalyzers / Roslynator rule(s)), then `.editorconfig` scoped escalation.
  - **Rollout**: start with `{BackendRoot}/{CorePrj}/**` as warnings; promote to error after remediation.

- **AB-0102: Exception correctness and specificity**
  - **Rule source**: `.cursor/rules/*.mdc` + `docs/rules/code-quality.md` (no generic `Exception`, correct argument names).
  - **Mechanism**: analyzer rules (argument exceptions, message patterns where feasible).
  - **Risk**: medium FP depending on rules; keep as warning unless very high signal.

- **AB-0103: “No magic numbers” / primitive obsession**
  - **Rule source**: `.cursor/rules/*.mdc` (enums over int literals for status/type).
  - **Mechanism**: likely not feasible with off-the-shelf analyzers alone; candidate for a custom Roslyn analyzer (higher maintenance) or keep as review-only.
  - **Suggested direction**: keep review-only until codebase stabilizes, then revisit.

## P1 — Conservative firewall expansions (only where FP stays low)

- **AB-0201: Service firewall — ban direct file IO in `{CorePrj}/Services`**
  - **Rule source**: `.cursor/rules/*.mdc` (services should stay testable; avoid environment coupling).
  - **Mechanism**: source-scan firewall tokens (`System.IO.File`, `System.IO.Directory`, `File.`, `Directory.`) inside `{BackendRoot}/{CorePrj}/Services/**`.
  - **False-positive risk**: low if scoped to Core services and tokens are specific.
  - **Rollout**: add detector test (happy/sad/edge) then gate.

- **AB-0202: Service firewall — ban direct environment/process coupling in Core services**
  - **Rule source**: `.cursor/rules/*.mdc` (no hardcoded config; avoid hidden environment coupling).
  - **Mechanism**: source-scan firewall tokens (`Environment.GetEnvironmentVariable`, `Process.Start`, etc.) in `{BackendRoot}/{CorePrj}/Services/**`.
  - **False-positive risk**: low–medium; needs careful token selection.

## P2 — Testing conventions (guideline → light automation)

- **AB-0301: Test naming convention lint**
  - **Rule source**: `docs/rules/testing.md` naming.
  - **Mechanism**: optional analyzer/linter for test projects; start as warning.
  - **Trade-off**: can create noise; only do if we agree the naming convention is stable.

- **AB-0302: AAA structure**
  - **Rule source**: `docs/rules/testing.md`.
  - **Mechanism**: not practical to enforce mechanically without a formatter-style tool; keep as review guideline.

## P2 — SQL “intent” rules (keep review-heavy; consider targeted automation)

- **AB-0401: SQL ownership (“one method, one purpose”)**
  - **Rule source**: `docs/rules/sql.md`.
  - **Mechanism**: difficult to automate reliably; keep as review guideline.
  - **Possible helper**: add repository-level metrics report (non-gate) to highlight growing repos or suspicious method counts.

