---
inclusion: fileMatch
fileMatchPattern: "src/**/Tests/**/*.cs"
---

# Testing Rules

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: add tests around changed code paths first. Avoid rewriting existing test suites just for style.
- **Strict (new project)**: enforce naming/AAA/isolation consistently from day 0.

### Legacy-safe “tolerance” strategy (recommended)

When onboarding a legacy codebase, prefer **blocking the worst risks first** over demanding full layering compliance on day 1.

- **IgnoreOnLegacy**: allow temporary exemptions in architecture tests (e.g., `[IgnoreOnLegacy]`) so you can:
  - enforce new/changed paths first
  - gradually shrink the ignored surface over time
- **Ban list first**: start with “must never happen” rules (e.g., API directly touches DB driver types, or returns driver exceptions/messages) before rolling out full allow-lists.

## Stack
Default in this repo: **NUnit** + **Moq**.

Cross-project guidance:

- Keep tests **framework-neutral** (AAA, naming, isolation) so projects can use NUnit/MSTest/xUnit.
- If you change the runner, keep the same *intent* and only swap attributes/assert APIs.

## Isolation
- Mock `IDapperContext` and all repository interfaces
- No real DB in unit tests
- Test Services only — never Controllers or Repositories directly

## Coverage (every public Service method)
1. Happy path — valid input, expected output
2. Validation failure — invalid input, expected exception
3. Persistence failure — simulate repository failure; exception must **surface** (verify rollback / leak protection according to the explicit UoW rules in [`transactions.md`](transactions.md))

## Naming
`MethodName_Scenario_ExpectedResult`
- `TransferStockAsync_ValidDto_ReturnsExpectedDto`
- `TransferStockAsync_RepositoryThrows_PropagatesException`
- `GetByIdAsync_InvalidId_ThrowsArgumentException`

## General
- No magic numbers — use enums and named constants
- Always structure with AAA sections (use comments if non-trivial)

## Acceptance-oriented gates (high signal)

- **Placeholder guard**: prevent “templates not initialized” false-green states (placeholders left behind).
- **Exception leak gate**: fail tests if API output includes DB driver types (`System.Data.*`, `Microsoft.Data.SqlClient.*`) or raw driver exception messages.

Bad (stop):

```csharp
// integration test disguised as a unit test (slow/flaky)
// - hits real DB
// - asserts via side effects only
```

Good (follow):

```csharp
// Arrange: mock repositories + IDapperContext
// Act: call the service method
// Assert: verify outputs and Commit/Rollback behavior
```
