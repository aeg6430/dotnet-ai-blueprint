---
inclusion: manual
---

# Request screening (rules)

This document defines a temporary request-screening pattern for blocking suspicious inbound traffic at the HTTP boundary.

Use it when a team needs a fast, reviewable mitigation for an active incident, such as:

- blocking a known abusive path or path prefix
- rejecting obviously malicious query keys or value fragments
- placing a narrow endpoint family behind a temporary control
- turning on report-only observation before enforcing a block

This is a bounded containment mechanism, not a general-purpose WAF replacement.

## Usage by phase

| Phase | Role | Recommended use |
|---|---|---|
| Maintenance | Probe | Use `ReportOnly` to observe and measure specific request shapes before deciding whether blocking is necessary. |
| Refactoring | Safety Net | Temporarily block or divert older request formats that the new path does not yet support safely. |
| Incident response | Tourniquet | Immediately cut off clearly abusive or risky traffic while the permanent fix is still being prepared. |

Treat these as temporary operational roles for the same API-edge control, not as three separate feature implementations.

## Design nature

Request screening is designed to be:

- **non-invasive** — it controls traffic at the HTTP edge without forcing immediate changes into deep business logic
- **temporary** — it buys time for safer rollout, refactoring, or incident containment; it is not meant to become the permanent home of business rules

Use it when the real need is: *"I need to control this traffic now, but I do not want to, or cannot yet, change the deeper business code safely."*

That also means there are clear cases where request screening is **not** the final answer:

- If the problem is long-lived, such as API performance optimization, fix the code path itself.
- If the rule is meant to be permanent, such as a new authorization requirement, implement it as a formal authorization policy or other first-class application behavior.
- If the behavior belongs to domain validation, orchestration, or data integrity, keep it in the proper Core/API design instead of leaving it as a screening rule.

## Default implementation model

Use **native ASP.NET Core** mechanisms:

- `middleware` for broad HTTP-path interception
- `filters` / `endpoint filters` only for narrow endpoint-specific variants
- normal `IOptions` / configuration binding for the toggle and match lists
- feature-local extension methods so registration stays easy to add and easy to remove

Do **not** make MediatR or another mediator framework the default implementation path for this feature.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: start with path-prefix blocking, query-key blocking, and report-only mode. Keep the first rollout narrowly scoped and easy to disable.
- **Strict (new project)**: define request screening from day 0 as a known incident-response extension point, with documented config shape, audit fields, and test coverage.

## Boundary and ownership

- The screening control belongs to the **API layer** at the HTTP entry point.
- It must run **before controllers / handlers** and before business orchestration reaches Core.
- It must **not** own business logic, validation rules, or repository/database decisions.
- It must **not** open a transaction or perform remote IO.

## Minimum recommended capability set

Prefer the smallest useful shape:

- `Enabled`
- `ReportOnly`
- blocked path prefixes
- blocked HTTP methods
- blocked query keys
- blocked query value fragments
- allowlisted path prefixes for critical health/admin paths
- stable `ProblemDetails` response shape
- structured audit / warning log entry for every block decision

Do not start with body parsing, regex-heavy inspection, or deep payload normalization unless the incident specifically requires it.

## Activation semantics

- Request screening is **opt-in**.
- It is inactive unless `RequestScreening:Enabled` is explicitly set to `true`.
- `Enabled = false` means disabled.
- Missing `RequestScreening` configuration also means disabled.
- `ReportOnly` has effect only after the feature is enabled.

## Suggested configuration shape

```json
{
  "RequestScreening": {
    "Enabled": true,
    "ReportOnly": false,
    "BlockStatusCode": 403,
    "ProblemType": "https://httpstatuses.com/403",
    "ProblemTitle": "Request blocked",
    "ProblemDetail": "This request has been blocked by a temporary security control.",
    "BlockedPathPrefixes": ["/api/legacy-import", "/api/debug"],
    "BlockedMethods": ["POST"],
    "BlockedQueryKeys": ["debug", "rawSql"],
    "BlockedQueryValueFragments": ["../", "<script"],
    "AllowlistedPathPrefixes": ["/health", "/ready"]
  }
}
```

## Composition pattern

Keep request screening packaged behind its own focused extension methods:

- `services.AddRequestScreening(configuration);`
- `app.UseRequestScreening();`

Recommended composition shape:

- the main DI file keeps **one line** to opt the feature in
- the feature-local extension owns options binding and related registration
- the middleware remains easy to remove after the temporary incident control is no longer needed

Avoid scattering request-screening registrations across the main composition root.

## Pipeline placement

Recommended order:

1. global exception boundary
2. HTTPS / forwarded headers as required by the host
3. **request screening**
4. routing / auth / controllers

If browser clients require cross-origin access to blocked responses, ensure the placement still cooperates with your chosen CORS policy.

## Response contract

When the control blocks a request:

- return a stable `ProblemDetails` response
- use a narrow, non-leaky title/detail
- include `Instance = Request.Path`
- prefer a reason code or machine-readable extension field over verbose raw input echoing

Do **not** reflect the suspicious query value or other attacker-controlled content back to the client.

## Audit and evidence

Each block or report-only hit should emit evidence that can support incident review:

- UTC timestamp
- request path and method
- correlation / trace ID
- actor identity if already available, otherwise `anonymous`
- matched rule type (`path`, `query-key`, `query-fragment`, etc.)
- screening mode (`report-only` or `blocking`)

Follow `docs/rules/audit-log.md` for the API-edge audit baseline.

## Operational workflow

For temporary use, document:

- why the control was enabled
- which paths / parameters are affected
- rollback trigger and rollback owner
- expiry or review time for the temporary rule
- follow-up cleanup issue / ADR / postmortem link

The configuration should be easy to remove after stabilization. Do not let temporary screening rules become permanent silent policy.

## Test guidance

At minimum, cover:

- blocked path returns the expected status and `ProblemDetails`
- allowlisted path still passes through
- report-only mode logs but does not block
- a blocked query key or value fragment is detected without echoing attacker-controlled content

## Relationship to other rules

- `docs/rules/audit-log.md` — audit baseline for API-edge interception
- `docs/rules/transactions.md` — keep the control outside transaction ownership
- `docs/rules/architecture-protocol.md` — keep Core orchestration explicit and free of HTTP-edge blocking concerns
