# Transactions (rules)

This rule set defines **transaction ownership** and the minimum safe behavior for both new projects and legacy integrations.

## Ownership (non-negotiable)

- **Boundary owns the transaction**: middleware / MVC filter / Minimal API endpoint filter.
- **Service must not own transactions**: services must not call `Begin/Commit/Rollback` and must not wrap business logic in manual transaction try/catch.
- **Repository never starts a transaction**: repositories only execute SQL using the ambient connection/transaction provided by the boundary.

### Who commits?

- **The boundary commits** when the request succeeds.
- **The boundary rolls back** on any exception and **rethrows**; the outer exception handler shapes the HTTP response.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**:
  - Treat legacy service/repository code as *do-not-touch*.
  - Roll out boundary transactions **endpoint-by-endpoint** (marker-based opt-in), not globally.
- **Strict (new project)**:
  - Make boundary transactions the default for write endpoints from day 0.

## Legacy caution: nested/manual transactions

If legacy code already opens transactions manually (e.g. `TransactionScope`, `BeginTransaction`, manual `Commit/Rollback`):

- Do **not** enable a global transaction boundary blindly.
- Prefer to **remove/centralize** service-level manual commits first, or scope rollout to selected endpoints.
- If you cannot clean it up yet, the boundary must be **re-entrant/idempotent**:
  - `Begin()` must not start a new transaction when one already exists (depth-based begin).
  - Any rollback is **fail-fast**: it invalidates the unit of work; later commits must fail.

## Audit/outbox independence (must persist)

If audit/outbox writes must survive a main transaction rollback:

- Use a **standalone connection** (not enlisted in the ambient transaction).
- Emit audit/outbox from the **outer exception boundary** (best-effort), or use an outbox pattern for durability.

