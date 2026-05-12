---
inclusion: manual
---

# External Integration Firewall

This rule defines how projects using this pack should integrate with **externally-owned or semantically unstable systems** without importing their runtime bugs, protocol drift, or dirty payload conventions into Core/Application code.

Examples include:

- third-party APIs and SDKs
- legacy internal shared services
- public-sector or vendor platforms
- systems with mixed XML/JSON payloads, unstable contracts, or weak environments

This is **not** a government-only rule. It applies to any integration where the other side is operationally or semantically untrusted.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: keep existing integrations working, but apply the firewall rules to new/touched adapters first. Prioritize transaction neutrality, error normalization, and evidence capture around unstable integrations.
- **Strict (new project)**: treat every external integration as an untrusted boundary from day 0. Require explicit adapter, translation, resilience, and logging design before use-case code depends on it.

## Core stance

- An external integration is **not** just an HTTP call or SDK invocation. It is a protocol boundary with unreliable semantics.
- The adapter layer must protect Core from:
  - dirty status semantics
  - dirty payload naming
  - dirty exceptions and transport quirks
  - dirty operational behavior (timeouts, retries, hanging calls, missing environments)
- A thin adapter is acceptable only for genuinely clean integrations. When semantics are not trustworthy, pair this rule with [`anti-corruption-layer.md`](anti-corruption-layer.md).

## Required defenses

### 1. Semantic normalization

- Do **not** assume success/failure is encoded in one stable `ResultCode`.
- External systems may signal outcome through any combination of:
  - HTTP status code
  - body-level code/flag fields
  - message text
  - nested envelopes
  - missing/contradictory fields
- The adapter must convert those signals into a stable, Core-facing meaning before returning.
- Do **not** let external code tables, magic strings, `dynamic`, `JObject`, raw XML nodes, or vendor SDK result objects cross into Core.

### 2. Data sanitization and canonicalization

- Sanitize external strings, dates, numbers, and enum-like values before Core sees them.
- Normalize inconsistent formatting such as:
  - full-width vs half-width text
  - locale-specific numbers/dates
  - padded identifiers
  - empty-string-as-null conventions
  - alternate calendar formats or malformed timestamps
- If a missing/dirty field would affect a business decision, do **not** silently invent a safe-looking value. Return `unknown`, reject the response, or raise a dependency/protocol error according to the integration contract.

### 3. Abnormal protocol handling

The firewall must explicitly handle ugly-but-common cases such as:

- `200 OK` with an error payload
- `500` or HTML body containing a valid business rejection
- inconsistent `Content-Type` vs actual body shape
- empty body on nominal success
- partial-success envelopes
- transport success but semantic failure

Adapters must not blindly call `EnsureSuccessStatusCode()` and deserialize directly into Core DTOs when the dependency does not deserve that trust.

### 4. Resilience and containment

All external integrations still obey [`resilience.md`](resilience.md):

- timeout
- bounded retry only when safe
- circuit breaker
- preflight `EnsureNoActiveTransaction()`

Do not scatter retry/backoff logic through use cases. Keep it in Infrastructure adapters/policies.

### 5. Transaction neutrality

All external integrations still obey [`transactions.md`](transactions.md):

- never hold a local DB transaction open while waiting on an external system
- perform remote verification/fetch first
- only then open the local short-lived transaction
- prefer outbox for post-commit side effects

The firewall does **not** exist to make unsafe transaction overlap acceptable. It exists to stop dirty external behavior at the edge.

### 6. Evidence-grade side-channel logging

Normal application logs and telemetry are not always enough for contract disputes.

For hostile or high-dispute integrations, provide a separate **evidence capture path** that can record:

- operation name
- correlation/request id
- endpoint/verb
- response status
- redacted/truncated request/response previews or a secure reference to stored evidence
- timestamps and duration

Rules:

- keep this separate from normal app logs when payload sensitivity or volume requires it
- redact secrets, credentials, tokens, identifiers, and raw payload fields per policy
- demonstrate masking of high-risk PII fields such as names, identity numbers, contact details, or equivalent project-specific identifiers
- do **not** dump full raw payload bodies into general-purpose logs by default
- prefer a stable evidence record shape so captured exchanges can later become replay fixtures, dispute artifacts, or adapter regression tests after approved redaction

See also [`audit-log.md`](audit-log.md).

### 7. Environment drift and documentation skepticism

External environments are often inconsistent across documentation, sandbox, staging, and production.

- Treat external docs as advisory, not authoritative.
- Prefer captured behavior, contract fixtures, and adapter tests over comments copied from vendor docs.
- Keep environment-specific deviations explicit in adapter options/config or ADR/spec notes.
- When sandbox/prod differ materially, document the gap and keep the drift handling in the adapter layer rather than leaking conditional behavior into Core.

## Normal telemetry vs evidence capture

Use **normal telemetry** for:

- latency
- retry count
- breaker state
- timeout failures
- transaction-overlap violations

Use **evidence capture** for:

- dispute-prone request/response exchanges
- semantically broken payloads
- vendor claims such as “we never received it”
- cases where response bodies or headers are operational evidence
- replay-oriented troubleshooting where a captured exchange should be reproducible in a local adapter test after approved redaction

## Starter shape

Copyable starter files for this rule live under `templates/`:

- `templates/BaseHttpAdapter.cs`
- `templates/ResiliencePolicies.cs`
- `templates/ExternalSystemWireModels.cs`
- `templates/ExternalSystemSanitizer.cs`
- `templates/ExternalSystemTranslator.cs`
- `templates/ExternalSystemExceptionTranslator.cs`
- `templates/ExternalSystemEvidenceLogger.cs`
- `templates/ExternalSystemGateway.cs`

Use `ServiceExtensions.cs` as the DI/composition reference for how the adapter and helper pieces should be registered in Infrastructure.

## Review checklist

When reviewing a new or changed external integration, ask:

1. Does the adapter normalize external semantics before Core sees them?
2. Are dirty payload names/shapes confined to Infrastructure wire models?
3. Is the adapter safe against `200-with-error-body`, malformed bodies, and contradictory signals?
4. Are timeout/retry/breaker rules in Infrastructure instead of use-case code?
5. Is remote IO kept outside active DB transactions?
6. Is there an appropriate evidence/logging strategy for disputes without leaking secrets?
7. If sandbox/prod differ, is that drift documented and contained at the boundary?

## Related rules

- [`anti-corruption-layer.md`](anti-corruption-layer.md)
- [`architecture-protocol.md`](architecture-protocol.md)
- [`cross-project-boundaries.md`](cross-project-boundaries.md)
- [`transactions.md`](transactions.md)
- [`resilience.md`](resilience.md)
- [`audit-log.md`](audit-log.md)
