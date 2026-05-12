# ADR-0001: Explicit short-lived UoW is the default transaction model

- **Status**: Accepted
- **Date**: 2026-05-12

## Context

This starter pack is meant to guide humans and AI assistants toward safe defaults for layered .NET backends.

The older "boundary-managed transaction" shape is convenient for short local CRUD paths, but it becomes risky when a request path includes remote IO, long-running work, or high concurrency:

- DB connections stay occupied longer than necessary
- lock duration increases
- connection-pool starvation becomes more likely
- transaction scope becomes harder to reason about when mixed with cross-system side effects

Because this pack targets copyable patterns, the default transaction model must optimize for safety under mixed workloads, not just convenience for the simplest endpoints.

## Decision

The default transaction model for this pack is:

- a scoped, lazy `IDapperContext`
- an explicit, short-lived transaction opened at the use-case/service boundary
- `Begin()` as late as possible
- `Commit()` / `Rollback()` immediately after the smallest local atomic DB write block
- no transaction for read-only/query flows
- no remote IO while the main DB transaction is active
- local business write plus local outbox insert in the same transaction when cross-system side effects are needed

Automatic HTTP-edge transaction wrappers are not the default. They are a narrow optional convenience only for short-lived, local-only write endpoints.

## Consequences

- **Positive**:
  - Better protection against long-lived transactions and pool starvation.
  - Clearer orchestration for AI-generated service/use-case code.
  - Better fit for outbox-based cross-system consistency.
  - Read-only flows avoid unnecessary transaction cost.
- **Negative / trade-offs**:
  - Services/use cases carry more explicit orchestration responsibility.
  - Teams lose some convenience compared with request-wide ambient transaction wrappers.
  - Legacy codebases may need gradual rollout instead of global enforcement on day 0.
- **Follow-ups**:
  - Keep `docs/rules/transactions.md` as the binding rule set.
  - Keep `templates/IDapperContext.cs`, `templates/DapperContext.cs`, `templates/StockService.cs`, and `templates/StockTransferUseCase.cs` aligned with this decision.
  - Prefer endpoint-by-endpoint rollout for legacy projects that still have manual or request-wide transaction behavior.

## Links

- Spec:
  - `docs/rules/transactions.md`
  - `docs/starter-pack/core/transactions.md`
  - `docs/ARCHITECTURE.md`
- Code:
  - `templates/IDapperContext.cs`
  - `templates/DapperContext.cs`
  - `templates/StockService.cs`
  - `templates/StockTransferUseCase.cs`
  - `templates/StockTransferService.cs`
- Related ADRs:
  - `docs/adr/0002-polly-style-outbound-resilience.md`
  - `docs/adr/0003-minimal-api-transaction-wrapper-limited-use.md`
