# Transactions (Starter Pack)

This starter pack assumes a **single transaction boundary at the HTTP edge** (controller/endpoint), and keeps application services free of transaction orchestration.

For the **binding rule set**, see: [`../../rules/transactions.md`](../../rules/transactions.md).

## Default strategy

- **Atomicity first**: no partial success inside one request transaction.
- **Exception means rollback**: services throw; the boundary rolls back.
- **Audit/outbox is independent**: audit/outbox writes must not be enlisted in the main transaction.

## What this strategy handles well

- **Nested service calls** inside a single request scope (transaction re-entrancy via depth counting).
- **Fail-fast rollback**: any failure invalidates the whole unit of work for this request.
- **Independent audit/outbox**: "must persist" side effects use a separate connection.

## Explicit limitations and recommended responses

### A. No partial success (No SAVEPOINT)

- **Why**: we keep the highest bar for atomicity.
- **If the business really needs “A fails but B must succeed”**:
  - Do **not** try to mix different lifecycles in the same DB transaction.
  - Convert **B** into an **asynchronous message** (e.g., outbox pattern), so it can succeed independently.

### B. MVC controller boundary only

- **Current state**: the transaction boundary is implemented as an MVC action filter (`TransactionActionFilter`).
- **If migrating to Minimal APIs**:
  - Extract the core logic into a reusable service (e.g., `TransactionService`).
  - Call it from an `IEndpointFilter` (Minimal API boundary), keeping semantics identical.

### C. Audit is best-effort by default

- **Current state**: audit failures should not block returning an error response.
- **If audit is extremely important**:
  - Write audit to a durable local sink first (e.g., file log / append-only log).
  - Use a background worker to sync to DB in batches (outbox-like), so audit survives transient DB failures.

## Team “military rules” (non-negotiables)

1. **No tag, no transaction**: if an action is not marked transactional, do not discuss transactions in services. Treat operations as auto-commit (or fail).
2. **No try/catch transaction in services**: services must not call `Begin/Commit/Rollback` or attempt to own transaction lifecycles.
3. **Exception means rollback**: when data is not right, throw; the boundary is responsible for rollback and error shaping.

