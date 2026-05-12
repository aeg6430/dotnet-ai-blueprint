---
inclusion: manual
---

# Automation Coverage Map (Backend)

This document maps **written rules** (`docs/rules/*.md` + `.cursor/rules/*.mdc`) to **enforceable automated checks** (tests/analyzers/CI).

> **Portable paths:** Examples use placeholders — adapt to your repository layout after the project setup flow described in [`docs/starter-pack/README.md`](../../starter-pack/README.md).
>
> | Placeholder | Typical meaning |
> |-------------|-----------------|
> | `{BackendRoot}` | Folder that contains backend `.csproj` trees (e.g. `src`, `src/backend`) |
> | `{CorePrj}` / `{InfraPrj}` / `{ApiPrj}` / `{TestsPrj}` | Project folder names (often match namespaces such as `Acme.Core`, `Acme.Infrastructure`, `Acme.Api`, `Acme.Tests`) |
> | `{ApiNamespace}` | API assembly root namespace (only `*.Extensions.*` may reference Infrastructure in many layering setups) |

**Legend**

- **Covered**: enforced as a hard gate (tests fail or build fails).
- **Partially covered**: some mechanical parts are enforced, but intent still needs review.
- **Not covered**: review-only today; candidate for analyzer/firewall automation later.

## Covered (enforced by tests)

- **Layering / assembly dependency direction**: `{TestsPrj}/Architecture/LayeringArchitectureTests.cs` (copy from [`docs/starter-pack/architecture-tests/`](../../starter-pack/architecture-tests/))
  - Core ↔ Infra ↔ Api boundaries
  - Api shell: only `{ApiNamespace}.Extensions.*` may reference Infrastructure
  - Core must not depend on `Dapper.*` / `Npgsql.*`

- **Persistence firewalls (source scan)**: `{TestsPrj}/Architecture/RepositoryFirewallArchitectureTests.cs`
  - **Repositories**: RF-001..RF-009
  - **Infrastructure adjunct folders** (Lookups/Helpers/TypeHandlers/Mappers/JWT/Security/Context): LF-001..LF-009

- **Service firewall (source scan)**: `{TestsPrj}/Architecture/ServiceFirewallArchitectureTests.cs`
  - SF-001..SF-008

- **API firewall (source scan)**: `{TestsPrj}/Architecture/ApiFirewallArchitectureTests.cs`
  - AF-001..AF-002

> **Operational note (endpoint protection):** these gates often rely on reflection, assembly loading, source scanning, and writing reports under `artifacts/`. If dev machines or CI build agents run Apex One or similar endpoint protection, coordinate a reviewed exclusion policy for the build workspace and required artifact output paths; otherwise test duration and pipeline stability can degrade for reasons unrelated to code quality.

## Covered (enforced by analyzers in build)

Backend builds enable analyzers via `Directory.Build.props`, with severity configured in `.editorconfig`.

- **Analyzer baseline enabled**: `Directory.Build.props`
  - `Meziantou.Analyzer`
  - `Roslynator.Analyzers`
- **Hard-error rules (current)**: `.editorconfig`
  - `MA0155 = error` (no `async void`)
  - `MA0042 = error` (no blocking calls in async methods) in:
    - `{BackendRoot}/{CorePrj}/**/*.cs`
    - `{BackendRoot}/{InfraPrj}/**/*.cs`
    - `{BackendRoot}/{ApiPrj}/**/*.cs` (with `{BackendRoot}/{ApiPrj}/Program.cs` often scoped to `silent`)
    - `{BackendRoot}/{TestsPrj}/**/*.cs`
  - `MA0147 = error` (avoid async void delegate) in:
    - `{BackendRoot}/{CorePrj}/**/*.cs`

These analyzer gates directly cover Cursor rule items such as **no `async void`**, **no `.Result`/`.Wait()` in async**, and (via firewalls) **no `Thread.Sleep`**.

## Partially covered (review + tests)

- **SQL ownership** (`docs/rules/sql.md`)
  - **Covered**: no `SELECT *`, no interpolated SQL strings, SQL-as-`const string` conventions (RF/LF firewalls).
  - **Not covered**: “one method, one caller purpose”, “new caller => new method”, and N+1 avoidance remain primarily review concerns today.

- **Mapping rules** (`docs/rules/mapping.md`)
  - **Covered**: some boundary constraints indirectly (e.g., Core not depending on persistence drivers; API composition boundaries).
  - **Partially covered**: “no manual mapping in Services/Repositories” is not currently enforced; relies on review and on keeping repositories/services thin via RF/LF/SF.

- **Testing rules** (`docs/rules/testing.md`)
  - **Covered** (project convention for enforcement work): when we add new architecture rules, we add detector tests with **happy/sad/edge** cases.
  - **Partially covered**: **Unit** quality (coverage thresholds, validation-path assertions, CI wiring) is **team-specific** — capture targets in your ADRs or test policy; starter-pack does not ship a single mandatory coverage ADR.
  - **Not covered**: AAA structure and generic test naming are usually not standalone automated gates unless you add a project-specific linter/policy.

- **Code review / learning mode** (optional team playbook, if you add one under `docs/rules/`)
  - **Not a gate** by design: process/format guidance, not compiler-enforced constraints.

## Not covered (needs review or analyzers)

- **Code quality heuristics** (`docs/rules/code-quality.md`, `.cursor/rules/*.mdc`)
  - Method length limits, nesting depth, guard clause ordering, “no nested ternary”, “method name contains And => split”, etc.
  - Some of these can be *partially* covered with analyzers, but require a deliberate selection and rollout plan to avoid noise.

- **Workflow constraints** (`.cursor/rules/*.mdc`)
  - Manual workflow triggers and process rules are not machine-enforced today.

- **Magic numbers / enum usage** (`.cursor/rules/*.mdc`)
  - Not automated today; candidates include analyzers or custom Roslyn rules (higher cost).

- **Exception message quality / `nameof()` usage** (`.cursor/rules/*.mdc`, `docs/rules/code-quality.md`)
  - Not automated today; some aspects can be covered via analyzers in scoped batches.

If we want stricter enforcement of these areas, the next step is adding **Roslyn analyzers + .editorconfig** and treating some warnings as errors in CI.

## Decision framework

- See [automation-decision-matrix.md](automation-decision-matrix.md) for how teams decide which rules become automated gates vs review guidelines, and how analyzer severities are rolled out.

## Analyzer baseline strategy (Phase A)

We intentionally roll out analyzers in a **predictable baseline**:

- **Enable analyzers for backend builds** via `Directory.Build.props`.
- **Default all analyzer diagnostics to warning** via `.editorconfig`:
  - `dotnet_analyzer_diagnostic.severity = warning`
- **Promote only a small set of high-signal rules to error**, and document any scoped exceptions.

### Example severities (document your repo’s `.editorconfig`; dates drift)

- **Error**
  - `MA0155` — `async void` (Meziantou): `dotnet_diagnostic.MA0155.severity = error`
- `MA0042` — blocking calls in async methods (Meziantou): **scoped error** in Core/Infrastructure/Api/Tests (see `.editorconfig`)
- `MA0147` — avoid async void delegate (Meziantou): **scoped error** in Core (see `.editorconfig`)
- **Warning**
  - `MA0137` — use `Async` suffix for awaitables (Meziantou)
  - `RCS1046` — async method name should end with `Async` (Roslynator)
- **Silent (scoped exception)**
  - `{BackendRoot}/{ApiPrj}/Program.cs`: `MA0042 = silent` (to avoid false positives on `app.Run()` hosting entrypoint)

### Upgrade path (Phase B later)

When we start Phase B, we will **escalate warnings to errors** incrementally by:

- Scoping first (e.g., only `{BackendRoot}/{CorePrj}/**` or `.../Services/**`)
- Fixing violations to zero in that scope
- Then promoting the chosen diagnostics to `error` for that scope in `.editorconfig`

