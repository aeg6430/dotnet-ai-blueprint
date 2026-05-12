# Feature Spec: Create Warehouse (Example)

> This is a **filled example** based on `feature-spec-template.md`.
> Use it to see how a plain-language request becomes an implementation-ready engineering spec.
> Replace names, routes, tables, and rules with your real feature details.

---

## Overview

Allow an authorized operations user to create a warehouse record from the back-office API so inventory locations can be registered before stock movement begins. The feature exists to replace spreadsheet-based warehouse onboarding with a validated and auditable API flow.

---

## Source Material

List the business-facing requirement sources that this spec was derived from.

- raw requirement file(s): `docs/requirements/raw/warehouse-onboarding-notes.md`
- stakeholder notes: operations manager requested "create warehouse before first inbound stock"
- ticket / issue: `OPS-142`

Known ambiguity from source material:

- the raw request says "manager approval may be needed later", but the current scope does not include an approval workflow

---

## Actors / Roles

List the roles involved in this feature.

| Role | Can do what |
|---|---|
| `WarehouseAdmin` | Create a warehouse and view the created record |
| `WarehouseViewer` | Read warehouse details only |
| `Auditor` | Review audit trail outside this feature's API |

---

## Use Cases

List the main flows in plain engineering language.

1. A `WarehouseAdmin` submits a valid warehouse create request and receives the created warehouse record.
2. A user submits an invalid request and receives a validation or conflict response without creating any data.
3. A `WarehouseViewer` or unauthorized caller attempts to create a warehouse and receives an authorization failure.

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/warehouses` | Create a new warehouse |
| `GET` | `/api/warehouses/{id}` | Get warehouse details by ID |

---

## Request Models

```text
POST /api/warehouses
{
  "code": string (required, uppercase, max 20, unique)
  "name": string (required, max 100)
  "location": string (required, max 200)
  "capacity": int (required, > 0)
  "statusId": int (optional, defaults to Active)
}
```

---

## Response Models

```text
WarehouseResponse
{
  "id": int
  "code": string
  "name": string
  "location": string
  "capacity": int
  "statusId": WarehouseStatus
  "createdAtUtc": string (ISO-8601)
  "createdBy": string
}
```

---

## Business Rules

- Warehouse `code` must be unique.
- `capacity` must be greater than `0`.
- `name` is required and trimmed before persistence.
- `location` is required and trimmed before persistence.
- If `statusId` is omitted, default to `Active`.
- Newly created warehouses cannot start in `Inactive` status unless a future approval feature explicitly allows it.
- This feature does not create stock balances; it only creates the warehouse master record.
- Business time must come from the project clock abstraction rather than direct `DateTime.UtcNow` calls inside business logic.

If a rule depends on time, environment, or external data, say so explicitly.

- `createdAtUtc` uses the system clock abstraction configured by the target project.

---

## Enums Required

List any enums this feature needs. If they don't exist yet, they must be created first.

| Enum | Values |
|---|---|
| `WarehouseStatus` | `Active = 1`, `Inactive = 2`, `UnderMaintenance = 3` |

---

## Database Tables

List the tables this feature reads from or writes to.

| Table | Operation |
|---|---|
| `Warehouse` | `SELECT`, `INSERT` |
| `AuditLog` | `INSERT` |

---

## External Dependencies

List any dependency outside the local database.

| Dependency | Direction | Purpose | Failure impact |
|---|---|---|---|
| none | n/a | This feature stays inside the local system boundary | n/a |

If none, state that explicitly.

- No outbound HTTP, MQ, SMTP, webhook, or file-storage dependency is part of this feature.

---

## Transaction Boundary

Describe what must happen atomically. If multiple tables are written, they must be in one transaction.

- Creating the `Warehouse` row and the local `AuditLog` row must succeed or fail together.
- The `GET /api/warehouses/{id}` flow is read-only and must not open a transaction.

Also answer these explicitly:

- Is this flow read-only? If yes, it must not open a transaction.
  - `POST /api/warehouses`: no
  - `GET /api/warehouses/{id}`: yes
- Does this flow call an external API / MQ / webhook / SMTP / file store? If yes, that work must not happen while the main DB transaction is active.
  - No
- If the feature emits cross-system side effects, what is the outbox strategy?
  - Not applicable in current scope
- What is the idempotency / dedupe strategy for retries?
  - Uniqueness on `code` prevents duplicate warehouse creation for the same business key; if the client retries after timeout, the API should return a conflict or the existing record according to target-repo policy

Example: Creating a warehouse and logging the audit entry must be committed together or rolled back together.

---

## Authorization

State who can call the feature and any special policy checks.

- authentication required? yes
- allowed roles / claims / permissions: `WarehouseAdmin` for `POST`, `WarehouseAdmin` or `WarehouseViewer` for `GET`
- ownership or tenant checks: if the target project is multi-tenant, uniqueness and reads are scoped to tenant

---

## Pagination

Does this feature return a list? If yes — which strategy?

- [ ] Cursor-based (`NextCursor`, `Limit`) — for high-traffic or mobile
- [ ] Offset-based (`Page`, `PageSize`, `TotalCount`) — for admin or back-office
- [x] No pagination needed

---

## Error Cases

| Scenario | Expected behaviour |
|---|---|
| Caller is not authenticated | `401 Unauthorized` |
| Caller lacks `WarehouseAdmin` permission on create | `403 Forbidden` |
| Warehouse not found on `GET` | `404 Not Found` |
| Duplicate warehouse code | `400 Bad Request` or `409 Conflict`, depending on target API standard |
| Capacity is `0` or negative | `400 Bad Request` with validation details |
| Unexpected persistence failure | safe error response; full exception logged internally |

---

## Observability

Define the minimum operational evidence needed.

- key log events:
  - warehouse create requested
  - warehouse create succeeded
  - warehouse create rejected due to validation or duplicate code
- audit event required? if yes, what action / target / actor should be captured?
  - yes; capture actor, action `CreateWarehouse`, target warehouse code / ID, result, and correlation metadata
- correlation / trace expectations:
  - request correlation ID must flow through API log and audit event
- metrics or counters, if any:
  - optional counter for successful warehouse creations
  - optional counter for duplicate-code rejections

Do not leave observability as an implicit "we will log something".

---

## Acceptance / Verification

Define how reviewers or testers will know the feature is correct.

- build / test command:
  - `dotnet build`
  - `dotnet test`
- smoke or manual verification path:
  - call `POST /api/warehouses` with a unique code, then call `GET /api/warehouses/{id}` using the returned ID
- sample input:

```text
POST /api/warehouses
{
  "code": "WH-TPE-001",
  "name": "Taipei Main Warehouse",
  "location": "Taoyuan Logistics Park",
  "capacity": 5000
}
```

- expected output:
  - `201 Created` with a warehouse payload containing `code = "WH-TPE-001"`
- expected DB or side-effect observation:
  - one new `Warehouse` row exists
  - one new `AuditLog` row exists with action `CreateWarehouse`

If test automation is not ready yet, document the manual verification path explicitly.

---

## Open Questions / Blind Spots

List anything that must be clarified before or during implementation.

- Should duplicate warehouse code return `400` or `409` under the target API contract?
- Should inactive warehouse creation remain blocked, or is that tied to a future approval workflow?

If this section is non-empty, do not pretend the feature is fully specified.

---

## Out of Scope

List anything explicitly NOT included in this feature to prevent scope creep.

- Stock initialization
- Bulk warehouse import
- Warehouse update and delete flows
- Approval workflow for inactive or restricted warehouse creation
