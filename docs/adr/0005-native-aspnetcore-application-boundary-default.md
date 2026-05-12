# ADR-0005: Native ASP.NET Core application boundary is the default model

- **Status**: Accepted
- **Date**: 2026-05-12

## Context

This starter pack is intended to be portable across Blueprint, Seed, and Target repositories without forcing every team to adopt the same third-party application framework stack.

The pack already standardizes the core application boundary shape:

- thin HTTP controllers in the API layer
- explicit service/use-case orchestration in Core
- explicit short-lived UoW ownership at the use-case/service boundary
- ASP.NET Core middleware / filters / decorators for cross-cutting concerns at the HTTP edge

That default shape is already documented across `docs/ARCHITECTURE.md`, `docs/rules/architecture-protocol.md`, `docs/rules/transactions.md`, and `.github/copilot-instructions.md`.

Introducing a mediator framework as the starter-pack default would change more than package choice:

- it introduces another abstraction that every exported Seed/Target repo must explain and govern
- it makes the default execution path less explicit for humans and AI assistants reading generated code
- it creates additional legal/procurement review for teams that treat dependency licensing as a delivery concern
- it overlaps with native ASP.NET Core mechanisms that already cover this pack's intended cross-cutting scenarios

At the same time, the concepts often associated with mediator-style architectures can still be useful:

- command/query separation
- focused handler-like classes
- thin request orchestration boundaries
- optional cross-cutting wrappers for narrow scenarios

The pack therefore needs a clear default that preserves those concepts without making a mediator framework the standard dependency.

Optional HTTP-boundary controls in particular should stay easy to adopt and easy to remove. If these features are spread across the main composition root, they become harder to review and harder to unwind after a temporary incident use case ends.

## Decision

This pack adopts **native .NET / ASP.NET Core mechanisms** as the default application-boundary model:

- HTTP boundary concerns use **middleware**, **MVC filters**, **endpoint filters**, and optionally **DI decorators**
- application orchestration uses **explicit services / use-case classes**
- transaction ownership remains **explicit** and **late-opened / early-committed** inside the use case/service
- optional boundary controls should be packaged behind focused extension methods so the main DI/composition root keeps a one-line opt-in registration and the feature remains easy to remove

`MediatR` is **not** a default dependency of this starter pack.

Mediator-style concepts are still allowed when they are implemented with native project types and remain readable within the pack's layering rules. For example, a team may use:

- explicit `Create*UseCase` / `Get*QueryService` classes
- strongly typed command/query DTOs
- focused decorators or filters for narrow cross-cutting behavior

If a specific Target repository already standardizes on MediatR, or has completed its own legal/architecture review, that repository may adopt it by **local exception ADR**. That decision is outside the portable starter-pack default.

## Consequences

- **Positive**:
  - Keeps the default control flow explicit for humans and AI assistants.
  - Avoids making the starter pack depend on an additional framework and licensing decision.
  - Aligns cross-cutting behavior with existing ASP.NET Core guidance already present in this pack.
  - Keeps transaction ownership and remote-IO boundaries visible in service/use-case code.
- **Negative / trade-offs**:
  - Teams do not get out-of-the-box mediator pipeline behaviors from the default pack.
  - Some request flows may contain more explicit wiring than a mediator-based design.
  - Repositories that already prefer MediatR must document their exception locally instead of inheriting it silently.
- **Follow-ups**:
  - Keep `.github/copilot-instructions.md` and `.cursor/rules/00-entrypoint.mdc` aligned with this default.
  - Keep API-edge cross-cutting guidance centered on native ASP.NET Core mechanisms.
  - Keep ADR-0001 and ADR-0003 as the controlling transaction-boundary decisions.

## Links

- Spec:
  - `docs/ARCHITECTURE.md`
  - `docs/rules/architecture-protocol.md`
  - `docs/rules/transactions.md`
  - `docs/rules/audit-log.md`
  - `docs/rules/request-screening.md`
  - `.github/copilot-instructions.md`
- Code:
  - `templates/Startup.cs`
  - `templates/ServiceExtensions.cs`
- Related ADRs:
  - `docs/adr/0001-explicit-short-lived-uow.md`
  - `docs/adr/0003-minimal-api-transaction-wrapper-limited-use.md`
