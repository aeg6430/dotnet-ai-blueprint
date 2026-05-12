# Resilience (rules)

This rule set defines the **runtime safety contract** for outbound dependencies: HTTP APIs, webhooks, message brokers, SMTP-like gateways, and similar remote systems.

Use this document together with [`transactions.md`](transactions.md): transaction rules prevent connection-pool starvation, while resilience rules prevent unstable dependencies from cascading through the service.

## Scope

- Applies to outbound adapters implemented in Infrastructure.
- Does **not** permit direct `HttpClient` / `RestClient` usage in Core services; the service firewall in [`architecture-protocol.md`](architecture-protocol.md) still applies.
- Covers timeout ordering, retry rules, circuit breaker usage, and preflight transaction checks.

## Core model

- Outbound calls belong in Infrastructure adapters behind Core ports/interfaces.
- Every outbound adapter must assume the remote dependency is **slow, lossy, and occasionally unavailable**.
- Every outbound adapter must assume it can be called during retries and duplicates unless the feature proves otherwise.

## Timeout ladder (non-negotiable)

Timeouts must be intentionally ordered so short local DB work fails before long remote waits dominate the request:

1. **DB command timeout inside the main UoW**
   - Default: `5s`
   - Allowed working range: `3s` to `5s` for normal OLTP writes
2. **Outbound dependency timeout**
   - Must be **greater than** the local DB command timeout
   - Typical starting point: `8s` to `15s` depending on the dependency SLA
3. **Overall request timeout / upstream deadline**
   - Must be **greater than** the outbound timeout
   - Must still be short enough to protect the service under dependency failure

If a feature needs slower thresholds, document the reason in a feature spec or ADR.

## Retries

- Retries must be **bounded**.
- Retries are safe only when one of these is true:
  - the remote operation is read-only
  - the remote operation is explicitly idempotent
  - the request carries an idempotency key / dedupe token
- Do **not** retry non-idempotent write operations blindly.
- Start with a small retry budget (for example, 2 retries after the initial attempt) and jitter/backoff; do not create retry storms.

## Circuit breaker

- Every remote dependency that can materially slow or fail requests should have a circuit breaker.
- The breaker should open on repeated transient failures or timeouts and stay open long enough to stop request pileups.
- When the breaker is open:
  - fail fast
  - surface a safe, dependency-aware error to the caller
  - emit enough telemetry to alert operators

## Preflight transaction checks

- Outbound adapters must call `EnsureNoActiveTransaction()` before issuing the remote request.
- The preflight check exists to prevent this anti-pattern:
  - open DB transaction
  - wait on slow/untrusted network
  - hold scarce connection/lock resources for the whole wait

### Recommended behavior

- **Development / test**: throw `InvalidOperationException` immediately.
- **Production**: do not silently continue. Either:
  - fail fast before sending the remote request, or
  - short-circuit to a controlled failure path defined by the feature

At minimum, production must log the violation with high severity and avoid hiding the bug.

## Polly-style adapter guidance

The starter-pack default for outbound HTTP clients is a Polly-style composition:

- timeout
- bounded retry with backoff/jitter
- circuit breaker

Recommended shape:

1. register a typed or named client in DI
2. attach policy handlers in Infrastructure
3. keep the policy wiring out of Core services

Do not scatter retry loops or ad hoc `try/catch` backoff logic through use cases.

## Idempotency

- If a remote operation may be retried, the feature must define its idempotency strategy.
- Typical options:
  - request id / idempotency key sent to the remote dependency
  - local dedupe table
  - outbox row with a stable message id
- Seed examples that call remote services should show the idempotency decision explicitly.

## Observability

At minimum, emit telemetry for:

- timeout failures
- retry attempts
- circuit breaker open/close state changes
- preflight transaction violations
- dependency latency

Telemetry must not leak secrets, access tokens, or full payload bodies.
