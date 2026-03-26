---
inclusion: fileMatch
fileMatchPattern: "src/**/Repositories/**/*.cs"
---

# SQL Rules

## Query Declaration
- Always `const string sql = @"..."` — never inline into Dapper calls
- Verbatim string `@"..."` for multiline — one clause per line

## Parameters
1. Manual params → `var parameters = new { }` declared separately
2. Whole entity → pass directly
3. ORDER BY direction → whitelist only: `var direction = isDescending ? "DESC" : "ASC";`

Never inline anonymous object directly into Dapper call when built from individual inputs.

## SQL Ownership
- One method, one purpose — returns exactly what its single caller needs
- Never modify existing SQL for a new caller — create a new method
- Never add JOINs/columns to existing query — new requirement = new method
- Name by purpose: `GetWarehouseByIdAsync` vs `GetWarehouseWithStockByIdAsync`
- Repository >5–7 methods → suggest splitting before adding more

Split guidance: `IWarehouseRepository` (CRUD) | `IWarehouseStockRepository` (stock ops) | `IWarehouseReportRepository` (reporting)

## SQL Safety
- Always parameterized — string interpolation forbidden
- Exception: ORDER BY direction only

## No Repository Calls in Loops 🚫
N+1 at Service layer — design a batch method instead:
```csharp
await _warehouseRepository.UpdateStockBatchAsync(items); // 1 query
```
Batch options: Dapper list param / TVP / multi-row INSERT / temp table + bulk copy

## IAsyncEnumerable
Use for large reports, exports, bulk pipelines. Not for small lookups or transactional queries.

## Proactive SQL Review (when inside a repository file)
Flag but never apply silently:
- `SELECT *` → suggest explicit columns
- N+1 pattern
- Large result sets without pagination
- Missing `WHERE` on large tables
- Unnecessary JOINs

Format: `💡 Optimization suggestion: [issue] in [method] — Approve to apply?`
