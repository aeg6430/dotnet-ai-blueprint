---
inclusion: fileMatch
fileMatchPattern: "src/**/Tests/**/*.cs"
---

# Testing Rules

## Stack
Framework: NUnit | Mocking: Moq | Location: `Project.Tests/ServiceTests/`

## Isolation
- Mock `IDapperContext` and all repository interfaces
- No real DB in unit tests
- Test Services only — never Controllers or Repositories directly

## Coverage (every public Service method)
1. Happy path — valid input, expected output
2. Validation failure — invalid input, expected exception
3. Transaction rollback — simulate repo failure, verify `Rollback()` called

## Naming
`MethodName_Scenario_ExpectedResult`
- `TransferStockAsync_ValidDto_CommitsTransaction`
- `TransferStockAsync_RepositoryThrows_RollsBackAndRethrows`
- `GetByIdAsync_InvalidId_ThrowsArgumentException`

## General
- No magic numbers — use enums and named constants
- Always structure with AAA sections (use comments if non-trivial)
