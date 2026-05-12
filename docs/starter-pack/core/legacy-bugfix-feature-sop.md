# Legacy Bugfix / Feature SOP

Use this playbook when you inherit a legacy .NET Framework / .NET Core MVC or Razor codebase that is already messy and the business expectation is "fix bugs and add features without destabilizing production".

This is a **legacy-safe** document. It assumes:

- large refactors are usually not approved
- `unit test` coverage is weak or effectively absent
- configuration, transactions, data access, and logging may already be inconsistent
- your first job is to reduce delivery risk, not to make the system pretty

For the overall adoption tracks, see [`../README.md`](../README.md). For the binding transaction and layering rules behind this SOP, see [`../../rules/transactions.md`](../../rules/transactions.md) and [`../../rules/architecture-protocol.md`](../../rules/architecture-protocol.md).

## Outcome first

Success on a legacy task usually means:

- the bug is reproducible before the fix
- the change is small enough to review and reason about
- the fix does not widen the blast radius unnecessarily
- the feature works on the intended path
- the next engineer can understand what was changed and how to verify it

It does **not** require broad cleanup, mass renaming, or retrofitting the entire codebase to modern standards in one pass.

## Default operating rules

When the system is already fragile, use these as the default guardrails:

- Do not introduce new hardcoded environment detection in business code.
- Do not add string-interpolated SQL or new ad hoc ADO.NET access in controllers or helpers.
- Do not place remote IO inside an active database transaction.
- Do not introduce sync-over-async (`.Result`, `.Wait()`) into changed paths.
- Do not let raw infrastructure or driver exceptions leak to clients.
- Do not mix `TransactionScope` into paths that already use `IDbTransaction`-style flows.

## The task loop

Use the same loop for bugfixes and small features.

### 1. Classify the request

Put the task into one primary bucket:

- **Bugfix**: existing behavior is wrong, unstable, or inconsistent
- **Small feature**: new behavior added to an existing flow
- **Risk-driven fix**: the main goal is to reduce incidents, not add visible product behavior

Then classify the path:

- **Read path**: query / page display / report / lookup
- **Local write path**: database write only
- **Cross-system write path**: local write plus email, HTTP call, MQ, file, webhook, or job dispatch

That classification matters because transaction handling differs sharply between them. Read paths should not open transactions; write paths should keep the transaction short and local. See [`../../rules/transactions.md`](../../rules/transactions.md).

### 2. Build an execution map before editing

Before changing code, write down the minimum path that explains the behavior:

1. entry point: controller, Razor page model, background job, scheduled task
2. main service or helper chain
3. repositories / ADO.NET / Dapper touchpoints
4. config reads and environment checks
5. external dependencies: HTTP APIs, SMTP, MQ, file share, cache
6. logs, exceptions, and user-facing error outputs

Do not try to map the whole application. Map only the path you are about to change.

## 3. Score the blast radius

Before touching code, check whether the path includes any of these:

- direct SQL in controller or page model
- multiple writes with no visible transaction boundary
- transaction wrapping remote IO
- machine-name or environment-name branching in application code
- static helper methods with hidden side effects
- duplicated validation logic in multiple layers
- swallowed exceptions or logs that omit context
- feature flags or config values that are read from more than one place

If two or more of these appear in the same path, default to a **smaller** implementation instead of a "clean" rewrite.

## 4. Choose the change shape deliberately

Prefer the smallest safe shape that improves the target path.

### Good default shapes

- fix a condition, query, mapping, or validation bug in place
- extract one focused helper to remove duplicated dangerous logic
- wrap a legacy dependency behind one interface for the changed path only
- move one DB access hotspot out of a controller into a repository/gateway
- centralize one config read instead of fixing all config problems globally
- insert one exception boundary or logging improvement at the path entry point

### Avoid by default

- renaming across the whole solution
- introducing a new generic abstraction layer with no immediate payoff
- converting the whole application to a new ORM or transaction model in one task
- rewriting unrelated modules "while we are here"

## 5. Define verification before the code change

When tests are weak, verification becomes part of the implementation.

Capture all of the following before editing:

1. **Reproduction**: exact input, account, record, page, API call, or sequence
2. **Expected result**: response, UI effect, DB mutation, side effect
3. **Observation points**: which table, log entry, audit event, or response field proves success
4. **Rollback signal**: what tells you the change is unsafe and should be backed out

If there is no reliable automated test yet, create at least one repeatable verification asset:

- a manual test checklist
- a curl / Postman / HTTP file request
- a SQL query used only for verification
- a small smoke test
- one regression test around the changed path, even if the rest of the system remains untested

The goal is not instant high coverage. The goal is to make the changed path **repeatable**.

## 6. Apply the transaction-first filter

For any write path, inspect the transaction behavior before anything else.

### Read paths

- do not open a transaction just to read data

### Local write paths

- validate first
- fetch remote information first if needed
- begin the transaction as late as possible
- write local data only
- commit immediately after the local atomic block

### Cross-system write paths

- never hold the DB transaction open while waiting on slow or untrusted remote systems
- prefer:
  1. validation / remote pre-check
  2. begin transaction
  3. local write + local outbox insert
  4. commit
  5. external delivery after commit

If you cannot introduce a full outbox yet, at minimum remove the remote call from the active transaction and document the residual risk.

## 7. Use the lowest-cost test ladder that still protects the task

Legacy-safe testing is about signal, not ideology. See [`../../rules/testing.md`](../../rules/testing.md).

Use the first level that is realistic for the current code path:

1. **Manual verification script**
   - best when the area has no stable test harness yet
2. **Smoke test**
   - best when one end-to-end check catches the main regression
3. **Focused regression test**
   - best when the bug is deterministic and the dependency seam already exists
4. **Unit test around changed logic**
   - best when you can isolate the service or helper without major surgery

Do not block critical bugfixes on building a full test pyramid from scratch. Do leave the path safer than you found it.

## 8. Leave breadcrumbs for the next task

Every legacy change should leave a small amount of durable context behind:

- what was changed
- how to verify it
- what was intentionally not cleaned up
- what risks remain in the surrounding path

Low-cost places to record that context:

- task description / PR description
- a short `KNOWN_RISKS.md` note
- backlog item for transaction or config cleanup
- a nearby code comment only when the trap is non-obvious

## 9. Deliverables per task

For each bugfix or feature in a legacy system, try to leave with this minimum package:

- one clear reproduction or acceptance note
- one clear verification method
- one bounded code change
- one explicit note about residual risk, if any

If the task touched a dangerous path, also leave one of these:

- a small regression test
- a smoke test command
- a documented verification query or scenario

## 10. Review checklist

Use this quick review list before merging a legacy change:

- Did we reproduce the problem before the fix?
- Is the changed path smaller than the full hotspot?
- Did we avoid adding new environment branching or config sprawl?
- Did we avoid widening or hiding transactions?
- Did we avoid remote IO inside active transactions?
- Did we add at least one repeatable verification path?
- Did we leave enough context for the next engineer?

## Practical standard

For legacy work, the standard is:

- **make the changed path safer**
- **make the behavior more observable**
- **do not accidentally modernize the wrong thing**

That is usually the fastest path to fewer incidents and more predictable delivery.
