# ADR-0003: Minimal API transaction wrappers are limited-use, not the default

- **Status**: Accepted
- **Date**: 2026-05-12

## Context

Minimal API endpoint filters can provide an attractive convenience wrapper for transaction handling. For short local write endpoints, they can reduce repetition and keep handlers compact.

However, once a handler includes outbound network IO, long-running computation, or cross-system side effects, an automatic boundary wrapper hides the real transaction lifetime. That makes it easier for humans and AI assistants to accidentally hold a DB transaction open across remote waits.

Because this pack prioritizes safe defaults, any automatic wrapper must stay explicitly secondary to the default explicit UoW model.

## Decision

Minimal API transaction filters/services remain optional and limited-use:

- they are allowed only for short-lived, local-only write endpoints
- they must not be used when the handler performs outbound network IO
- they must not be used when the handler performs long-running CPU work
- they must not be used when side effects should flow through outbox/background dispatch
- the default recommendation remains explicit, late-opened, early-committed UoW orchestration inside the use case/service

This pack keeps Minimal API transaction wrappers as an optional teaching module, not as the primary architecture pattern.

## Consequences

- **Positive**:
  - Preserves a convenience option for simple Minimal API write endpoints.
  - Prevents the optional wrapper from becoming a misleading default for mixed IO flows.
  - Keeps the pack internally consistent with the explicit short-lived UoW decision.
- **Negative / trade-offs**:
  - Teams using Minimal APIs still need judgment about when the optional wrapper is safe.
  - Some endpoints may need refactoring away from the wrapper as features evolve and add remote dependencies.
  - More documentation is required so the optional path is not mistaken for the standard path.
- **Follow-ups**:
  - Keep `docs/starter-pack/optional/minimal-api/transactions.md` explicit about these constraints.
  - Keep `TransactionService.cs.txt` and `TransactionEndpointFilter.cs.txt` framed as narrow convenience helpers only.
  - If a Minimal API path adds outbound IO later, move transaction ownership back into the explicit use case/service flow.

## Links

- Spec:
  - `docs/starter-pack/optional/minimal-api/transactions.md`
  - `docs/starter-pack/core/transactions.md`
  - `docs/rules/transactions.md`
- Code:
  - `docs/starter-pack/optional/minimal-api/TransactionService.cs.txt`
  - `docs/starter-pack/optional/minimal-api/TransactionEndpointFilter.cs.txt`
  - `templates/StockTransferUseCase.cs`
- Related ADRs:
  - `docs/adr/0001-explicit-short-lived-uow.md`
  - `docs/adr/0002-polly-style-outbound-resilience.md`
