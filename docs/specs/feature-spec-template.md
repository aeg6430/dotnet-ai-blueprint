# Feature Spec: {Feature Name}

> Save this file as `docs/specs/{feature-name}.md`.
> The AI reads this before writing any code. Be specific — vague specs produce blind spots.
> Treat `docs/requirements/raw/` as source material, not as the final engineering contract.
> Delete any section that is not applicable.

---

## Overview

One paragraph describing what this feature does and why it exists.

---

## Source Material

List the business-facing requirement sources that this spec was derived from.

- raw requirement file(s):
- stakeholder notes:
- ticket / issue:

If the raw requirement is ambiguous, say so here instead of hiding the ambiguity in implementation.

---

## Actors / Roles

List the roles involved in this feature.

| Role | Can do what |
|---|---|
| `Admin` | Example: create and update warehouses |
| `Viewer` | Example: read only |

---

## Use Cases

List the main flows in plain engineering language.

1. Primary happy path
2. Important failure path
3. Optional admin / back-office path

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/warehouses/{id}` | Get warehouse by ID |
| `POST` | `/api/warehouses` | Create a new warehouse |

---

## Request Models

```text
POST /api/warehouses
{
  "name": string (required, max 100)
  "location": string (required)
  "capacity": int (required, > 0)
}
```

---

## Response Models

```text
WarehouseResponse
{
  "id": int
  "name": string
  "location": string
  "capacity": int
  "statusId": WarehouseStatus (enum)
}
```

---

## Business Rules

- List each rule explicitly — one bullet per rule
- Example: A warehouse cannot be deactivated if it has active stock
- Example: Capacity must be greater than current stock level before reducing

If a rule depends on time, environment, or external data, say so explicitly.

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
| `Warehouse` | SELECT, INSERT, UPDATE |
| `Audit` | INSERT |

---

## External Dependencies

List any dependency outside the local database.

| Dependency | Direction | Purpose | Failure impact |
|---|---|---|---|
| `Inventory API` | outbound HTTP | Example: validate stock state | request fails / retry / degrade |
| `SMTP` | outbound | Example: send notification | non-blocking / post-commit |

If none, state that explicitly.

---

## Transaction Boundary

Describe what must happen atomically. If multiple tables are written, they must be in one transaction.

Also answer these explicitly:

- Is this flow read-only? If yes, it must not open a transaction.
- Does this flow call an external API / MQ / webhook / SMTP / file store? If yes, that work must not happen while the main DB transaction is active.
- If the feature emits cross-system side effects, what is the outbox strategy?
- What is the idempotency / dedupe strategy for retries?

Example: Creating a warehouse and logging the audit entry must be committed together or rolled back together.

---

## Authorization

State who can call the feature and any special policy checks.

- authentication required?
- allowed roles / claims / permissions:
- ownership or tenant checks:

---

## Pagination

Does this feature return a list? If yes — which strategy?

- [ ] Cursor-based (`NextCursor`, `Limit`) — for high-traffic or mobile
- [ ] Offset-based (`Page`, `PageSize`, `TotalCount`) — for admin or back-office
- [ ] No pagination needed

---

## Error Cases

| Scenario | Expected behaviour |
|---|---|
| Warehouse not found | `404 Not Found` |
| Duplicate warehouse name | `400 Bad Request` |
| Insufficient capacity | `400 Bad Request` with descriptive message |

---

## Observability

Define the minimum operational evidence needed.

- key log events:
- audit event required? if yes, what action / target / actor should be captured?
- correlation / trace expectations:
- metrics or counters, if any:

Do not leave observability as an implicit "we will log something".

---

## Acceptance / Verification

Define how reviewers or testers will know the feature is correct.

- build / test command:
- smoke or manual verification path:
- sample input:
- expected output:
- expected DB or side-effect observation:

If test automation is not ready yet, document the manual verification path explicitly.

---

## Open Questions / Blind Spots

List anything that must be clarified before or during implementation.

- Example: Should duplicate warehouse names be unique per tenant or globally?
- Example: Is the notification mandatory or best-effort?

If this section is non-empty, do not pretend the feature is fully specified.

---

## Out of Scope

List anything explicitly NOT included in this feature to prevent scope creep.

- Example: Bulk warehouse import is not part of this feature
- Example: Warehouse deletion is handled in a separate feature
