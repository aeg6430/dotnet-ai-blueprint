---
inclusion: manual
---

# Official Architecture Protocol — SOLID & layered responsibility

This document is written to be **copyable**: the *principles* are stable even if the solution name changes.

**Status:** Binding engineering contract for backends adopting this layered pack.  
**Principle:** Obey SOLID and explicit layering. No improvised, unstructured edits (“vibe coding”). Run a **static architecture review** in your head before emitting code: which layer owns this, and which abstraction carries it?

**Placeholders:** `{CoreNamespace}`, `{InfrastructureNamespace}`, `{ApiNamespace}`, `{TestsNamespace}`, `{BackendRoot}`, `{Solution}` — replace via the project setup flow described in [`docs/starter-pack/README.md`](../starter-pack/README.md) and the repository root `README.md`; path semantics match [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md).

---

## Adoption profile (legacy-safe vs strict)

When integrating into a legacy codebase, assume existing services and repositories may already violate these rules.

- **Legacy-safe (default)**: treat existing code as *do-not-touch*. Apply new rules to **new/changed code only** and enforce the transaction/UoW rules first.
- **Strict (new project)**: enforce the full rule set globally from day 0 (CI + analyzers + architecture tests).

**Migration notes (legacy is messy):**

- Prefer **UoW-first** changes (remove long-lived/request-wide transactions, keep remote IO out of active transactions) over broad refactors of deep business logic.
- Avoid enabling global transaction boundaries if services already open transactions manually; roll out endpoint-by-endpoint and/or ensure the boundary is re-entrant (depth-based begin) and fail-fast on rollback.

## 1. SRP — single responsibility

### 1.1 Repository (persistence gate)

**Allowed**

- SQL (literals as `const string`, parameterized arguments).
- Dapper execution and materialization into **projection/row types** under `{InfrastructureNamespace}/Persistence/Models/`.

**Forbidden**

- Business rules or use-case branching.
- Transformation intended for API or domain consumption (beyond what Dapper does automatically for a single row type).
- `JsonSerializer` (or manual JSON parsing) **inside repository classes**.
- Clock or environment coupling such as `DateTime.Now` / `DateTime.UtcNow` for domain logic (logging is not an excuse to hide business time).
- **Private mapping methods** that assemble Core DTOs, or **nested private** row types — use named types under `Persistence/Models/`, then **one** thin call to a Helper or Mapper.

Bad (stop):

```csharp
// repo mixes IO + business rules + JSON parsing + mapping
var json = JsonSerializer.Deserialize<Foo>(row.Json);
if (json.IsExpired) return null;
return new FooDto(json.Id, json.Value);
```

Good (follow):

```csharp
// repo is a thin persistence gate:
// query -> row -> single delegation (helper/mapper)
return FooMapping.ToDtos(rows);
```

**Mapping policy**

- **Mapperly** (`{InfrastructureNamespace}/Mappers/` for persistence-facing maps, `{ApiNamespace}/Mappers/` for API-specific maps; `[Mapper]` partials): preferred for **declared** entity/DTO/object-graph maps per `docs/rules/mapping.md`.
- **Infrastructure/Helpers** (or **TypeHandlers**): persistence-specific work (e.g. JSON columns, legacy shapes) that Mapperly does not own.
- The repository stops at: *query → row type → single delegation* (e.g. `return SomeMapping.ToDtos(rows);`).

### 1.2 Service (orchestrator)

**Owns**

- Business rules, validation at the use-case level, and orchestration across repositories within the explicit, short-lived unit of work defined by [`transactions.md`](transactions.md). Services may coordinate the UoW boundary for local atomic DB work, but they must not hide remote IO inside an active transaction.

**Data cleansing / formatting**

- Prefer **Infrastructure/Helpers** (or injected abstractions) so service methods stay readable and testable.

**Outputs**

- Return **well-defined Core DTOs** to the API layer. Do not leak **Infrastructure entities** or persistence row types above Infrastructure.

**Command / write result semantics**

- Public Core write use cases (`Create*`, `Update*`, `Delete*`, `Approve*`, etc.) must return a **strongly typed, semantically meaningful result** instead of `bool`, bare `Task`, or `void`.
- Use the result type to represent **expected business outcomes** such as success, validation failure, conflict, rejection, or not-found. Do not collapse command outcomes into a single true/false flag.
- **Unexpected system faults** (database errors, transaction violations, timeout/dependency failures, serialization bugs, etc.) must still throw and flow to the global exception boundary / logging policy. Do **not** hide them behind a generic `Failure` result.
- This rule applies to the **public application boundary** of command/use-case services, not every private/helper/repository method. Technical probe methods such as `ExistsAsync(...)` may still return `bool` when that is the clearest contract.

---

## 2. OCP — open for extension, closed for unrelated modification

- Extend behavior with **new types or new mapper/helper methods** rather than patching unrelated classes with new `if` branches and field copies.
- **Mapperly:** When models gain fields, extend the mapper — avoid duplicated hand assignments in services or repositories.
- **JSON / serialization:** Centralize options and format assumptions in **Helpers** or shared infrastructure; do not scatter serializer details through business code.

---

## 3. LSP & ISP — substitutable, focused contracts

- **Depend on interfaces:** Controller → `IService`; Service → `IRepository` (and other abstractions). Constructors take interfaces, not concrete implementations (DIP — §4).
- **Avoid fat interfaces:** Prefer smaller, capability-focused interfaces so implementors are not forced to carry unrelated operations.

---

## 4. DIP — dependency inversion

**Tooling isolation**

| Mechanism | Location | Role |
|-----------|----------|------|
| **Helpers** | `{InfrastructureNamespace}/Helpers/` | Reusable persistence-side tools: JSON mapping, parsing, normalization — **not** embedded in repository classes. |
| **Mapperly** | `{InfrastructureNamespace}/Mappers/` (and `{ApiNamespace}/Mappers/` when API-only) | Compile-time, maintainable object-to-object mapping per domain conventions. |
| **TypeHandlers** | `{InfrastructureNamespace}/TypeHandlers/` | Dapper-specific type materialization when appropriate. |

**Anti-pattern**

- **Ad-hoc “middle” logic:** More than a few lines of per-field copying or manual parsing **inside** a repository or service **without** extraction to a Mapper or Helper. If the block grows, **extract**.

---

## 5. Data access standard

- **No `SELECT *`:** List columns explicitly; alias to match projection property names.
- **Parameterized SQL:** Use the database’s idioms — e.g. PostgreSQL `= ANY(@Ids::uuid[])` for UUID sets; avoid building dynamic `IN (...)` from concatenation.
- **Scalar typing:** When needed for stable mapping (e.g. `COUNT`), cast explicitly (e.g. `::int`) so Dapper maps predictably.

---

## 6. Runtime

- This pack assumes **.NET 8** backends unless your repository standard differs. Do not depend on APIs or C# features that exist only in **.NET 9+** unless the repository explicitly upgrades.

---

## 7. Relationship to other project rules

- **`docs/ARCHITECTURE.md`** — folder layout and cross-cutting patterns.
- **`docs/rules/cross-project-boundaries.md`** — how to collaborate with differently-styled internal projects without importing their coupling into Core/Application boundaries.
- **`docs/rules/external-integration-firewall.md`** — the defensive integration rule for semantically hostile or runtime-unstable external systems.
- **`docs/rules/anti-corruption-layer.md`** — how adapters/translators keep foreign payloads, names, and exceptions from leaking into Core.
- **[Cursor rules under `.cursor/rules/`](../../.cursor/rules/)** — stable, versioned, and only supported read order for Cursor agents. Keep them consistent with this document.

---

## 8. Automated architecture enforcement (Phase 1–6)

These checks are **binding** for backend changes: they run as normal tests in **`{TestsNamespace}`** and should stay green in CI/local workflows.

### Phase 1 — Layering (ArchUnitNET)

- **Intent:** enforce dependency direction across assemblies (`Core` ↔ `Infrastructure` ↔ `Api`), aligned with §4 (DIP).
- **Tests:** `{BackendRoot}/{TestsNamespace}/Architecture/LayeringArchitectureTests.cs`
- **ADR trail:** `docs/adr/` (see ADR index in `docs/adr/README.md`)

### Phase 2–3 — Repository firewall (source scan, no exemptions)

- **Intent:** keep repositories “thin persistence gates” (see §1.1) using deterministic, repo-local rules that catch common regressions early.
- **Tests:** `{BackendRoot}/{TestsNamespace}/Architecture/RepositoryFirewallArchitectureTests.cs`
- **Policy:** violations must be fixed in code. There is **no** allowlist or bypass file for repository firewall rules.

#### Repository firewall rule IDs

- **RF-001:** No `System.Text.Json` surface area inside `Repositories/*.cs` (move JSON work to Helpers/TypeHandlers; keep repositories string/row typed).
- **RF-002:** No nested types inside repository classes (extract row/projection types to `{InfrastructureNamespace}/Persistence/Models/`).
- **RF-003:** No `private Map*` methods inside repositories (mapping belongs in Mapperly/Helpers).
- **RF-004:** No `SELECT *` (list columns explicitly).
- **RF-005:** No EF Core primitives in repositories (`DbContext`, `DbSet<>`, `IQueryable<>`, etc.) — persistence is **Dapper-first**.
- **RF-006:** No `DateTime.Now` / `DateTime.UtcNow` inside repositories (avoid sneaky “business time” in persistence code).
- **RF-007:** No `Thread.Sleep` / `async void` inside repositories.
- **RF-008:** SQL text should be promoted to compile-time constants named `sql` or `*Sql` (e.g. `const string lockSql = @"...";`). This rule is intentionally heuristic: it targets obvious inline/literal SQL smells without pretending to fully parse C#.
- **RF-009:** No interpolated SQL strings (`$"..."` / `$@"..."`) in repositories (SQL must be parameterized).

### Phase 4 — Composition tightening (ArchUnitNET)

- **Intent:** keep HTTP controllers free of Infrastructure references (composition stays in `Extensions/` / DI), keep persistence drivers (`Dapper`, `Npgsql`) out of `{CoreNamespace}`, and confine **Infrastructure `using`/type dependencies** in the API layer to `{ApiNamespace}.Extensions` (DI composition).
- **Tests:** same file as Phase 1 — `{BackendRoot}/{TestsNamespace}/Architecture/LayeringArchitectureTests.cs`
- **Rules (summarized):**
  - Types under `{ApiNamespace}.Controllers` must **not** depend on `{InfrastructureNamespace}`.
  - Types whose full name starts with `{ApiNamespace}.` but **not** `{ApiNamespace}.Extensions.` must **not** depend on `{InfrastructureNamespace}`.
  - Types under `{CoreNamespace}` must **not** depend on types in the `Dapper.*` or `Npgsql.*` namespaces (the analyzer loads those assemblies so dependencies are visible).

### Phase 5 — Persistence adjunct: Lookups + Helpers (source scan, no exemptions)

- **Intent:** `Infrastructure/Lookups` may participate in SQL/Dapper like repositories; `Infrastructure/Helpers` holds persistence-side JSON/normalization per §4. Both use the **same adjunct rule set (LF-*)**: aligned SQL and hygiene checks **without** banning JSON (unlike **RF-001** on repositories).
- **Tests:** `{BackendRoot}/{TestsNamespace}/Architecture/RepositoryFirewallArchitectureTests.cs` (scan roots: `Lookups/`, `Helpers/`, `TypeHandlers/`, `Mappers/`, `JWT/`, `Security/`, `Context/`)
- **Policy:** same as repository firewall — fix violations in code; no allowlist.

#### LF rule IDs (Lookups and Helpers)

- **LF-001:** No `SELECT *` (same intent as **RF-004**).
- **LF-002:** No EF Core primitives (same intent as **RF-005**).
- **LF-003:** No `DateTime.Now` / `DateTime.UtcNow` (same intent as **RF-006**).
- **LF-004:** No `Thread.Sleep` / `async void` (same intent as **RF-007**).
- **LF-005:** SQL literals as `const string sql` / `const string *Sql` (same heuristic as **RF-008**).
- **LF-006:** No nested types inside adjunct types (same heuristic as **RF-002**); row/DTO shapes belong in `{InfrastructureNamespace}/Persistence/Models/` (or other declared models), not nested inside lookups/helpers when avoidable.
- **LF-007:** No `private Map*` methods (same heuristic as **RF-003**).
- **LF-008:** No `TimeProvider.System` direct usage outside `SystemClock` — time must flow via `IClock`.
- **LF-009:** No interpolated SQL strings (`$"..."` / `$@"..."`) in persistence adjunct folders.

### Phase 6 — Service + API firewalls (source scan, no exemptions)

- **Intent:** make a subset of `.cursor/rules/*.mdc` / `docs/rules/*` enforceable as tests, focusing on low-false-positive boundaries.
- **Tests**:
  - `{BackendRoot}/{TestsNamespace}/Architecture/ServiceFirewallArchitectureTests.cs`
  - `{BackendRoot}/{TestsNamespace}/Architecture/ApiFirewallArchitectureTests.cs`

#### Service firewall rule IDs

- **SF-001:** Core services must not reference persistence driver primitives (`Dapper`, `Npgsql`, `IDbConnection`, etc.).
- **SF-002:** Core services must not use blocking/async pitfalls (`Thread.Sleep`, `async void`, `.Result`, `.Wait()`).
- **SF-003:** Core services must not read time directly (`DateTime.Now/UtcNow` or `TimeProvider.System`); use `IClock`.
- **SF-004:** Core services must not reference direct external IO client primitives (`HttpClient`, `IHttpClientFactory`, `System.Net.Http`, etc.); external IO must flow via Core ports (interfaces) with adapters in Infrastructure.
- **SF-005:** Core services must not reference direct file system primitives (`File.*`, `Directory.*`, etc.); file system access must flow via Core ports (interfaces) with adapters in Infrastructure.
- **SF-006:** Core services must not reference host environment/process primitives (`Environment.*`, `Process.*`, etc.); use ports/adapters.
- **SF-007:** Core must not depend on ASP.NET Core host primitives (e.g., `IFormFile`, `Microsoft.AspNetCore.*`). File ingress must be handled at the boundary; Core receives `Stream`/named `byte[]` only. See [`file-upload.md`](file-upload.md).
- **SF-008:** Public Core write use cases must not return ambiguous success/failure contracts such as `bool`, bare `Task`, or `void`. Return a strongly typed, semantically meaningful result for expected business outcomes; unexpected system faults still throw to the global exception boundary.

#### API firewall rule IDs

- **AF-001:** API controllers must not reference `{InfrastructureNamespace}` types directly (Infrastructure wiring stays in `{ApiNamespace}.Extensions` / DI composition).
- **AF-002:** API must not use sync-over-async primitives (`.Wait(...)`, `.GetAwaiter().GetResult(...)`, `invocation.Result`). Enforced as a source-scan in `{BackendRoot}/{TestsNamespace}/Architecture/ApiFirewallArchitectureTests.cs`.

### How to run

```bash
dotnet test path/to/{Solution}.sln -c Release
```
