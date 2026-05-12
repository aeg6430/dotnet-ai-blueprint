# ADR-0002: Polly-style outbound resilience is the default HTTP adapter model

- **Status**: Accepted
- **Date**: 2026-05-12

## Context

This pack now treats outbound dependencies as first-class failure domains. External APIs, webhooks, and similar remote systems are slow, lossy, and occasionally unavailable.

If the starter pack does not define a default resilience shape, teams and AI assistants will tend to invent inconsistent retry loops, ad hoc timeout handling, or no circuit breaker at all. That creates operational drift and raises the risk of retry storms or request pileups.

The pack already enforces that remote IO must not overlap with an active DB transaction. The outbound dependency model must complement that rule with a repeatable runtime policy.

## Decision

The default outbound HTTP adapter model for this pack is a Polly-style composition attached in Infrastructure:

- typed or named `HttpClient`
- timeout policy
- bounded retry policy with backoff/jitter
- circuit breaker policy
- preflight `EnsureNoActiveTransaction()` check before the outbound request

Core services do not reference `HttpClient` directly and do not own retry/circuit-breaker logic.

Retries are allowed only for read-only or explicitly idempotent remote operations, or when the request carries a stable idempotency key / dedupe token.

## Consequences

- **Positive**:
  - A single, copyable resilience shape for outbound HTTP dependencies.
  - Better alignment between transaction safety and dependency failure handling.
  - Easier for AI assistants to generate adapter code without inventing new retry conventions.
  - Clearer telemetry expectations for latency, retries, breaker state, and transaction-overlap violations.
- **Negative / trade-offs**:
  - Adds dependency and configuration overhead compared with a bare `HttpClient`.
  - Some dependencies may need feature-specific thresholds or exceptions documented in a spec/ADR.
  - Circuit-breaker and retry settings still need tuning per SLA; the starter pack provides defaults, not magic values.
- **Follow-ups**:
  - Keep `docs/rules/resilience.md` as the binding rule set.
  - Keep `templates/BaseHttpAdapter.cs`, `templates/ResiliencePolicies.cs`, `templates/InventoryGateway.cs`, `templates/PricingGateway.cs`, and `templates/ShipmentGateway.cs` aligned with this decision.
  - When a remote write is retryable, show the idempotency strategy explicitly in templates and feature specs.

## Links

- Spec:
  - `docs/rules/resilience.md`
  - `docs/rules/transactions.md`
  - `docs/ARCHITECTURE.md`
- Code:
  - `templates/BaseHttpAdapter.cs`
  - `templates/ResiliencePolicies.cs`
  - `templates/InventoryGateway.cs`
  - `templates/PricingGateway.cs`
  - `templates/ShipmentGateway.cs`
  - `templates/ServiceExtensions.cs`
- Related ADRs:
  - `docs/adr/0001-explicit-short-lived-uow.md`
  - `docs/adr/0003-minimal-api-transaction-wrapper-limited-use.md`
