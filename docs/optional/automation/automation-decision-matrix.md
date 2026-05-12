---
inclusion: manual
---

# Automation Decision Matrix (Backend)

This document defines how a team adopting this pack decides whether a **written rule** should become an **automated gate** (tests/analyzers) versus remain a **review guideline**.

> **Portable paths:** `{BackendRoot}`, `{CorePrj}`, `{InfraPrj}`, `{ApiPrj}`, `{TestsPrj}`, `{ApiNamespace}` — see [automation-coverage.md](automation-coverage.md).

## 1) Decision axes

When proposing a new automated rule (test/analyzer), score it on these axes:

- **Coverage gain**: Does it turn a frequent review miss into an automatic failure?
- **False-positive risk**: How likely is it to flag correct code?
- **Remediation cost**: How many files are likely to change to comply?
- **Maintenance cost**: Will we need frequent exceptions, tuning, or rule rewrites?
- **Consistency**: Does it align with `docs/rules/architecture-protocol.md` and existing Phase rules?

## 2) Allowed enforcement mechanisms

- **ArchUnitNET**: best for assembly/type dependency boundaries (low false positives).
- **Source-scan firewalls (regex heuristics)**: best for “obviously wrong” patterns in specific folders (Repositories, adjunct folders, Services, Controllers).
- **Roslyn analyzers + `.editorconfig`**: best for cross-cutting code quality (async pitfalls, usage, naming, some design rules).

## 3) Baseline policy (how strict we are)

### Baseline (always-on)

Keep most diagnostics at **warning**; only promote a small set of high-signal rules to **error**.

Current baseline:

- **Automated tests** (hard gate):
  - Phase 1–6 rules in `docs/rules/architecture-protocol.md` §8 (ArchUnit + RF/LF/SF/AF).
- **Roslyn analyzers** (build-time):
  - Enabled for backend projects via `Directory.Build.props` + `.editorconfig`.
  - Default analyzer severity: **warning**.

### Escalation policy (warning → error)

Promote a rule to **error** only when:

- It has **low false-positive risk**, and
- The remediation is **small** or can be constrained to a folder scope, and
- We can document a clear, stable exception strategy (file- or folder-scoped severity).

Examples:

- `async void` is almost always wrong in ASP.NET Core service/repository code ⇒ can be **error** (with suppression for legitimate event handlers if needed).
- “Async suffix everywhere” can be noisy in Controllers ⇒ keep **warning**, or only escalate inside Core/Infrastructure.

## 4) Current enforcement inventory (matrix)

### A) Architecture boundaries (ArchUnitNET)

- **Mechanism**: ArchUnit
- **Scope**: Assemblies / namespaces
- **Coverage gain**: High
- **False positives**: Low
- **Maintenance**: Low
- **Status**: **Gate**
- **Where**: `{TestsPrj}/Architecture/LayeringArchitectureTests.cs` (from starter-pack architecture tests)

### B) Persistence firewalls (RF/LF)

- **Mechanism**: Source scan (regex)
- **Scope**:
  - Repositories: `Infrastructure/Repositories` (RF-*)
  - Adjunct: `Infrastructure/Lookups|Helpers|TypeHandlers|Mappers|JWT|Security|Context` (LF-*)
- **Coverage gain**: High
- **False positives**: Low–Medium (mitigated by heuristic-only rules)
- **Maintenance**: Medium (heuristic tuning)
- **Status**: **Gate**
- **Where**: `{TestsPrj}/Architecture/RepositoryFirewallArchitectureTests.cs`

### C) Service + API firewalls (SF/AF)

- **Mechanism**: Source scan (regex)
- **Scope**:
  - Core services: `{CorePrj}/Services` (SF-*)
  - API controllers: `{ApiPrj}/Controllers` (AF-*)
- **Coverage gain**: Medium–High
- **False positives**: Low (rules intentionally conservative)
- **Maintenance**: Low–Medium
- **Status**: **Gate**
- **Where**:
  - `{TestsPrj}/Architecture/ServiceFirewallArchitectureTests.cs`
  - `{TestsPrj}/Architecture/ApiFirewallArchitectureTests.cs`

### D) Roslyn analyzers (SDK + OSS)

- **Mechanism**: Roslyn analyzers + `.editorconfig`
- **Scope**: Backend build (cross-cutting)
- **Coverage gain**: Medium–High
- **False positives**: Medium (varies by rule)
- **Maintenance**: Medium (severity tuning)
- **Status**: **Mixed** (few errors, most warnings)
- **Where**:
  - `Directory.Build.props`
  - `.editorconfig`

#### Current analyzer set (commercial-friendly OSS)

- **Meziantou.Analyzer** (MIT)
  - Example enforced: `MA0155` (**error**) — no `async void`
  - Example monitored: `MA0042` (**warning**) — avoid blocking calls in async contexts (file exception for ASP.NET hosting entrypoint)
- **Roslynator.Analyzers** (Apache-2.0)
  - Example monitored: `RCS1046` (**warning**) — async methods should end with `Async`

## 5) How to propose a new automated rule

1. Identify the written rule source (`.cursor/rules/*.mdc` / `docs/rules/*` / spec).
2. Choose the mechanism (ArchUnit vs source-scan vs Roslyn analyzer).
3. Score the decision axes and justify any expected false positives.
4. Decide default severity (error/warning) and any scoped exceptions.
5. Update [`docs/rules/architecture-protocol.md`](../../rules/architecture-protocol.md) §8 (if it introduces new rule IDs) and [automation-coverage.md](automation-coverage.md) if coverage changes.

