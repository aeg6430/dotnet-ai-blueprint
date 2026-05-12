---
inclusion: manual
---

# Cross-Project Boundary Governance

This rule explains how a project using this pack should collaborate with **other internal projects that use different architectural styles** without importing their coupling, transaction bugs, or error-handling habits into its own Core/Application boundary.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: do not try to restyle neighboring projects. Keep their current shape intact and add translation/wrapping only at the integration boundary.
- **Strict (new project)**: keep this project internally consistent from day 0 and require all cross-project integrations to enter through explicit contracts, adapters, and error/telemetry rules.

## Core stance

- Do **not** force global style unification across unrelated projects.
- Do **not** mirror another project's internal implementation style inside this project just because that project already exists.
- Treat this project as a **green zone**: the inside obeys this pack's layering and runtime rules; the outside is integrated through contracts and adapters.

## What may differ vs what must not differ

### Differences we can tolerate

- Folder layout
- Naming conventions
- Controller/page thickness in legacy hosts
- Test naming or fixture style
- UI stack or host shape (MVC, Razor Pages, API-only, worker, etc.)

### Differences we must not import

- Direct Infrastructure leakage into Core
- Request-wide or long-lived transactions
- Remote IO inside active DB transactions
- Silent-failure command contracts (`bool`, bare `Task`, `void` on public write use cases)
- Shared DTO/entity/model leakage across project boundaries
- Hidden dependency retries, timeout assumptions, or untracked side effects
- Missing correlation/error semantics across integration calls

## Contract-first collaboration

When collaborating with a differently-styled project, align on the **contract**, not the implementation.

At minimum, the contract should define:

- request/response DTOs
- expected business failures vs unexpected system faults
- timeout expectations
- idempotency / retry behavior for write operations
- correlation / trace identifiers
- authentication / caller identity expectations
- versioning expectations when the integration evolves

Do **not** share persistence entities, database row models, or framework host types across projects.

## Boundary pattern

### Inbound (they call us)

- Keep the public entrypoint familiar to the caller if needed (HTTP API, message contract, shared facade).
- Translate incoming models at the boundary into this project's use-case DTOs / commands.
- Normalize foreign error codes or odd payload conventions before they reach Core services.

### Outbound (we call them)

- Put all translation in Infrastructure adapters/gateways.
- Convert this project's clean request/result model into whatever foreign shape the target system expects.
- Normalize foreign failures, partial-success patterns, and unstable payloads before returning to Core.

This is a practical anti-corruption layer even if the code does not literally use that name.

## Shared library policy

Shared libraries are the most common source of style leakage.

- Prefer sharing **stable technical infrastructure** (authentication plumbing, logging bootstrap, tracing setup) over sharing domain-specific models.
- If a shared library carries legacy/static/global/host-coupled behavior, wrap it in Infrastructure and expose a clean interface to Core.
- Do **not** let `Common.Lib` types leak into Core DTOs, service interfaces, or public command results.
- It is acceptable to duplicate a small DTO or translation object rather than couple two projects to the same unstable shared model.

## Error and telemetry alignment

Cross-project collaboration is usually broken more by runtime semantics than by field names.

- Expected business outcomes should map to explicit result/error contracts at the boundary.
- Unexpected dependency faults must remain visible to the project's global exception/logging boundary.
- Outbound adapters must preserve correlation/trace identifiers where possible.
- Document timeout and retry expectations explicitly; do not assume the other project shares this pack's defaults.
- If the other project has weak logging/telemetry, treat it as an unreliable signal source and add observability on this side of the boundary.

## Transaction and side-effect rules still apply

Integration pressure is **not** an exemption from this pack's runtime rules.

- Do not hold a local DB transaction open while waiting on another internal project.
- If cross-project side effects must happen after a local write, prefer local write + outbox + post-commit delivery.
- If the foreign system is slow or unreliable, contain that behavior in the outbound adapter and resilience policy layer.

See also:

- [`anti-corruption-layer.md`](anti-corruption-layer.md)
- [`external-integration-firewall.md`](external-integration-firewall.md)
- [`architecture-protocol.md`](architecture-protocol.md)
- [`transactions.md`](transactions.md)
- [`resilience.md`](resilience.md)
- [`audit-log.md`](audit-log.md)

## Review checklist

When a PR adds or changes a cross-project integration, ask:

1. Does Core still depend only on interfaces and clean DTOs?
2. Are foreign models translated at the boundary instead of flowing inward?
3. Are error, timeout, retry, and correlation expectations explicit?
4. Is any shared library wrapped before it reaches Core?
5. Did we preserve local transaction rules and avoid remote IO overlap?
