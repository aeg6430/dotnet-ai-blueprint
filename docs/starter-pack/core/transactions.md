# Transactions (Starter Pack)

This starter pack assumes a **scoped, lazy `IDapperContext`** plus an **explicit, short-lived transaction** around the smallest local atomic DB write block.

For the **binding rule set**, see: [`../../rules/transactions.md`](../../rules/transactions.md). For outbound dependency policies, see [`../../rules/resilience.md`](../../rules/resilience.md).

## Default strategy

- **Open the connection late**: the DB connection should not be opened at request entry.
- **Open the transaction later**: `Begin()` happens only when the local atomic write block is ready to run.
- **Commit early**: local business writes and outbox insert complete, then the connection is released quickly.
- **Keep remote IO outside the transaction**: verification/fetch first, commit local state next, dispatch side effects through outbox/background work.

## What this strategy handles well

- **High-concurrency write paths** where connection-pool pressure matters.
- **Nested repository work** inside one local atomic block because repositories share the same scoped `IDapperContext`.
- **Cross-system side effects** via outbox instead of direct remote calls inside the main transaction.
- **Read-heavy APIs** because query flows do not pay the cost of an unnecessary transaction.

## Decision guide

### A. Read-only query

- Use `IDapperContext.Connection`
- Do **not** call `Begin()`
- Do **not** attach a transaction helper/filter

### B. Pure local multi-table write

- Validate input
- `Begin()`
- Execute local writes
- `Commit()` / `Rollback()`

An `ExecuteAsync(...)` helper is acceptable here if it wraps only the local atomic DB work.

### C. External API + local write

- Perform remote verification/fetch first
- Only then start the local transaction
- Write business data + outbox row in the same transaction
- Commit
- Let a worker/dispatcher handle the side effect

## Explicit limitations and recommended responses

### A. No partial success inside the main transaction

- If the business really needs “A fails but B must succeed”:
  - do **not** mix those lifecycles in one DB transaction
  - turn **B** into an outbox-backed asynchronous step

### B. Automatic HTTP-edge transaction wrappers are limited-use

- MVC/Minimal API filters are **not** the default in this pack.
- Keep them only for short-lived, local-only write endpoints with no remote IO and no long-running work.
- If a path touches `HttpClient`, message publish, or similar outbound work, do not use the automatic wrapper for that path.

### C. Audit is best-effort by default

- Audit that must survive rollback should use a durable local sink, standalone connection, or outbox-like flow.

## Team rules (non-negotiables)

1. **No transaction for read-only flows**: `GET` / query use cases do not call `Begin()`.
2. **No remote IO while the DB transaction is active**.
3. **Begin late, commit early**.
4. **Outbox shares the same `IDapperContext` transaction as the business write**.
5. **Dispose must rollback leaked transactions**.

