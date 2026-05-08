---
inclusion: manual
---

# Audit Log (rules)

This document defines a minimal, copyable baseline for audit logging in layered .NET services.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: start by enforcing a single audit interception point at the HTTP boundary and log only high-signal events (authentication, authorization, write operations, and failures). Avoid refactoring deep legacy code for logging style.
- **Strict (new project)**: define audit requirements (fields, retention, correlation) early and keep the audit pipeline consistent from day 0.

## Principles

- **Single interception point**: audit should be captured in one consistent place (HTTP boundary and/or global exception boundary), not scattered across services.
- **Correlation**: audit entries must be correlatable to request tracing (`TraceId`, user identity if available, path/method).
- **Do not leak sensitive data**: audit must not record secrets, credentials, or raw connection strings.
- **Do not couple to business transactions**: if audit must persist even on rollback, use a standalone connection/outbox strategy.

## Minimal required fields (recommended)

- Timestamp (UTC)
- Actor identity (subject, or "anonymous"/system)
- Action (operation name)
- Target (resource identifier)
- Result (success/failure)
- Trace/Correlation ID
- Source (IP, user-agent) where appropriate

## Implementation guidance (non-prescriptive)

- Prefer a **boundary-owned** approach:
  - **HTTP edge**: middleware / filters / endpoint filters capture request metadata.
  - **Global exception boundary**: capture failures consistently.
- For durability requirements, prefer an outbox pattern or a dedicated audit sink using standalone connection semantics.

