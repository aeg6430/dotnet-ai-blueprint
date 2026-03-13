# Feature Spec: {Feature Name}

> Save this file as `docs/specs/{feature-name}.md`.
> The AI reads this before writing any code. Be specific — vague specs produce blind spots.
> Delete any section that is not applicable.

---

## Overview

One paragraph describing what this feature does and why it exists.

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/warehouses/{id}` | Get warehouse by ID |
| `POST` | `/api/warehouses` | Create a new warehouse |

---

## Request Models

```
POST /api/warehouses
{
  "name": string (required, max 100)
  "location": string (required)
  "capacity": int (required, > 0)
}
```

---

## Response Models

```
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

## Transaction Boundary

Describe what must happen atomically. If multiple tables are written, they must be in one transaction.

Example: Creating a warehouse and logging the audit entry must be committed together or rolled back together.

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

## Out of Scope

List anything explicitly NOT included in this feature to prevent scope creep.

- Example: Bulk warehouse import is not part of this feature
- Example: Warehouse deletion is handled in a separate feature
