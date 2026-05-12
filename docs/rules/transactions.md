# Transactions (rules) — Database connections are expensive and scarce; external networks are slow and untrusted. Never let them overlap in the same synchronous path.

This rule set defines the **binding unit-of-work contract** for Dapper-based services in this pack. The default model is a **scoped, lazy `IDapperContext`** plus an **explicit, short-lived transaction** around the smallest local atomic write block.

## Core model (non-negotiable)

- **`IDapperContext` is scoped per request/use-case** so collaborating repositories can share one connection/transaction when needed.
- **Connection opening is lazy**: do not open the database connection when the request starts; open it only on the first actual SQL call.
- **Transactions are explicit and short-lived**: `Begin()` happens immediately before the local atomic write block; `Commit()`/`Rollback()` happen immediately after it.
- **Repositories never own connections or transactions**: repositories must always use the current `IDapperContext.Connection` and `IDapperContext.Transaction`.
- **Remote IO and active DB transactions must never overlap**: no `HttpClient`, `RestClient`, message publish, file upload, or other slow/untrusted IO while `IsInTransaction` is `true`.

## Hard rules

### 1. Read-only paths do not start transactions

- `GET` / query / lookup flows must **not** call `Begin()`.
- Read models should use `IDapperContext.Connection` directly and execute SQL without a transaction unless the database engine requires a special case that is documented in a spec/ADR.

### 2. Begin as late as possible

- Complete validation, mapping, idempotency lookups, and remote calls **before** opening the transaction.
- The transaction should cover only the local atomic write block.
- If a use case mixes remote IO and local writes, the correct default is:
  1. remote verification / remote fetch
  2. `Begin()`
  3. local data writes + outbox insert
  4. `Commit()`

### 3. No remote calls while a transaction is active

- If a service or adapter needs outbound network IO, it must call `EnsureNoActiveTransaction()` before sending the request.
- Outbound adapter implementations should fail loudly in development/test when a live transaction is detected.
- Production code must **not silently continue** after detecting this violation; fail fast or short-circuit according to [`resilience.md`](resilience.md).

### 4. Outbox atomicity is mandatory for cross-system side effects

- If a write flow must trigger email, MQ, webhook, or another service, the main DB transaction may contain only:
  - local business data changes
  - local outbox insert
- The outbox write must use the **same** `IDapperContext.Connection` and `IDapperContext.Transaction` as the business write.
- Repositories must not open their own connection/transaction for outbox work, or atomicity is broken.

### 5. No silent transaction leaks

- `IDapperContext.Dispose()` / `DisposeAsync()` must detect an active transaction.
- If dispose happens while `IsInTransaction == true`, the implementation must:
  - rollback the transaction
  - release the connection
  - emit a **critical** log entry because the unit of work leaked

## Recommended `IDapperContext` contract

At minimum, the context should make these behaviors explicit:

- `IDbConnection Connection { get; }`
- `IDbTransaction? Transaction { get; }`
- `bool IsInTransaction { get; }`
- `int DefaultCommandTimeoutSeconds { get; }`
- `void Begin()`
- `void Commit()`
- `void Rollback()`
- `void EnsureNoActiveTransaction()`
- optional helper: `Task ExecuteAsync(Func<Task> work, CancellationToken cancellationToken = default)`

### Helper methods

- `ExecuteAsync(...)` is allowed as **syntax sugar** for local atomic DB work.
- It must wrap `Begin -> work -> Commit` and `Rollback` on failure.
- If it runs inside an already-active transaction, it must participate in the ambient UoW without committing it early.
- It must **not** be used to wrap remote IO, long-running computation, or cross-system orchestration.

## Timeout expectations

- **Short local transaction** means short SQL timeout.
- Default guidance for SQL executed inside the main UoW is **3-5 seconds**; use `5` seconds unless a feature spec says otherwise.
- Longer timeouts require a spec or ADR because they increase lock time and pool occupancy.
- See [`resilience.md`](resilience.md) for the full timeout ladder across DB, outbound HTTP, and request lifetime.

## Automatic boundaries: limited-use, not the default

HTTP filters / endpoint filters / middleware that auto-open a transaction are **not** the default strategy in this pack.

They are acceptable only when **all** of the following are true:

- the endpoint is a short-lived local write use case
- no outbound network IO occurs inside the handler
- no long-running CPU work occurs inside the handler
- the team explicitly wants opt-in convenience for these narrow paths

If any outbound IO exists, do **not** use an automatic transaction boundary for that path.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**:
  - Treat legacy service/repository code as *do-not-touch* unless the feature already changes it.
  - Remove or isolate request-wide transaction filters before promoting this rule set globally.
  - Prefer endpoint-by-endpoint rollout for explicit short-lived UoW adoption.
- **Strict (new project)**:
  - Use the explicit short-lived UoW model from day 0.
  - Keep filters/decorators optional and narrow; never let them hide remote IO inside a live transaction.

## Nested/manual transaction caution

If legacy code already opens transactions manually (e.g. `TransactionScope`, `BeginTransaction`, manual `Commit/Rollback`):

- Do **not** add a second automatic boundary on top.
- Prefer to centralize the transaction in one explicit use-case/service path.
- If simple nesting must exist temporarily:
  - `Begin()` should be re-entrant/idempotent
  - any rollback is **fail-fast**
  - later commits must fail once the UoW is invalidated

## Audit/outbox independence (must persist)

If audit/outbox writes must survive a main transaction rollback:

- Use a **standalone connection** or durable local sink for the independent audit path.
- Prefer an outbox pattern for durable cross-system side effects.
- Do not mix "must survive rollback" work into the main business transaction.

