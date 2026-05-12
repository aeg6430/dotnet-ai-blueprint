---
inclusion: manual
---

# Audit Log (rules)

This document defines a minimal, copyable baseline for audit logging in layered .NET services.

For teams operating under controls such as **ISO 27001**, treat this as a baseline for traceability and centralized security-relevant activity monitoring rather than an optional observability add-on.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: start by enforcing a single audit interception point at the HTTP boundary and log only high-signal events (authentication, authorization, write operations, and failures). Avoid refactoring deep legacy code for logging style.
- **Strict (new project)**: define audit requirements (fields, retention, correlation) early and keep the audit pipeline consistent from day 0.

## Principles

- **Single interception point**: audit should be captured in one consistent place at the API entry point (HTTP boundary and/or global exception boundary), not scattered across services.
- **Correlation**: audit entries must be correlatable to request tracing (`TraceId`, user identity if available, path/method).
- **Actor + action clarity**: audit entries should clearly show who attempted what operation, against which target, and with what result.
- **Do not leak sensitive data**: audit must not record secrets, credentials, or raw connection strings.
- **Do not couple to business transactions**: if audit must persist even on rollback, use a standalone connection/outbox strategy.

## Minimal required fields (recommended)

- Timestamp (UTC)
- Actor identity (`UserId`, subject, or `anonymous`/system)
- Action (operation name / attempted behavior)
- Target (resource identifier)
- Result (success/failure)
- Trace/Correlation ID
- Source (IP, user-agent) where appropriate

## Implementation guidance (non-prescriptive)

- Prefer a **boundary-owned** approach:
  - **HTTP edge**: middleware / filters / endpoint filters capture request metadata and act as the canonical audit interception point for API traffic.
  - **Global exception boundary**: capture failures consistently.
- For high-signal operations, ensure the audit record includes actor identity, action, target, result, and correlation metadata even when the business operation later fails.
- For durability requirements, prefer an outbox pattern or a dedicated audit sink using standalone connection semantics.
- When an integration is dispute-prone, keep raw request/response evidence in a **separate redacted evidence channel** rather than dumping full payloads into general application logs. See [`external-integration-firewall.md`](external-integration-firewall.md).

