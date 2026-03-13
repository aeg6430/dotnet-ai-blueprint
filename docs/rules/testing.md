# Testing Rules

> Read this file when writing or reviewing unit tests.

---

## Stack

- **Framework**: NUnit
- **Mocking**: Moq
- **Location**: `Project.Tests/ServiceTests/`

---

## Isolation

- Mock `IDapperContext` and all repository interfaces — Services must be fully testable without a real database
- Never test repositories with a real database connection in unit tests
- Never test Controllers directly — test Services only

---

## Coverage Requirements

Every public Service method must have tests covering:

1. **Happy path** — valid input, expected output
2. **Validation failure** — invalid input, expected exception
3. **Transaction rollback** — simulate repository failure, verify `Rollback()` is called

---

## Naming Convention

Test method names must follow:
`MethodName_Scenario_ExpectedResult`

Examples:
- `TransferStockAsync_ValidDto_CommitsTransaction`
- `TransferStockAsync_RepositoryThrows_RollsBackAndRethrows`
- `GetByIdAsync_InvalidId_ThrowsArgumentException`

---

## No Magic Numbers in Tests

The no magic numbers rule applies equally in tests — use enums and named constants, never raw literals.

---

## Arrange / Act / Assert

Always structure tests with clear AAA sections. Use comments if the test is non-trivial:

```csharp
// Arrange
// Act
// Assert
```
