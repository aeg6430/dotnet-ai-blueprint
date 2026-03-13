# SQL Rules

> Read this file when working on repositories, Dapper queries, or SQL.
> See `templates/WarehouseRepository.cs` for code examples of all cases below.

---

## Query Declaration

- Always `const string sql = @"..."` — never inline query strings into Dapper calls
- Use verbatim string `@"..."` for multiline queries — one clause per line for readability

---

## Parameter Declaration

Three valid cases — see templates for examples:

1. **Manual parameters** → always declare as `var parameters = new { }` separately before passing to Dapper
2. **Whole entity** → pass directly, no wrapper needed
3. **ORDER BY direction** → cannot be parameterized, use whitelist interpolation:
   `var direction = isDescending ? "DESC" : "ASC";` — never trust raw user input

**Never** inline an anonymous object directly into a Dapper call when built from individual inputs.

---

## SQL Ownership

- **One method, one purpose** — a repository method returns exactly what its single caller needs
- **Never modify existing SQL for a new caller** — create a new dedicated method instead
- **Never add JOINs or columns to an existing query** — new requirement = new method
- **Name methods by purpose** — `GetWarehouseByIdAsync` vs `GetWarehouseWithStockByIdAsync`

**Repository size warning**: If a repository exceeds ~5-7 methods → stop and suggest splitting by feature context before adding more.

**Feature split guidance:**

| Repository | Owns |
|---|---|
| `IWarehouseRepository` | Core CRUD |
| `IWarehouseStockRepository` | Stock-specific operations |
| `IWarehouseReportRepository` | Read-heavy / reporting queries |

---

## SQL Safety

- Always use parameterized queries — string interpolation in SQL is forbidden
- Exception: ORDER BY direction only — whitelist interpolation is acceptable

---

## No Repository Calls Inside Loops 🚫

Never call a repository method inside a `for`, `foreach`, or `while` loop — this is an N+1 problem at the Service layer.

**Wrong:**
```csharp
foreach (var item in items)
{
    await _warehouseRepository.UpdateStockAsync(item.SkuId, item.Quantity); // N queries
}
```

**Correct:** Design a batch repository method that accepts a collection and executes a single SQL operation:
```csharp
await _warehouseRepository.UpdateStockBatchAsync(items); // 1 query
```

**Batch SQL options (choose based on volume and scenario):**
- **Dapper list parameter** — pass `IEnumerable<T>` directly; Dapper expands `IN (@list)` automatically for simple cases
- **Table-Valued Parameter (TVP)** — for large batches or complex multi-column updates in SQL Server
- **`INSERT ... VALUES` multi-row** — build a single parameterized multi-row insert
- **Temp table + bulk copy** — for very large datasets (1000+ rows)

**During code review:** if a repository call is found inside a loop → flag as 🚫 N+1, suggest the appropriate batch strategy above.

---

## IAsyncEnumerable

Use `IAsyncEnumerable<T>` with `await foreach` for streaming large result sets row by row.

- ✅ Suitable: large reports, data exports, bulk processing pipelines
- ❌ Not suitable: small lookups, single-record queries, anything requiring a transaction

---

## SQL Optimization — Proactive Review

When working inside a repository file, proactively review existing SQL and suggest optimizations — but **never apply silently**.

Checklist:
- `SELECT *` → suggest explicit column names
- N+1 patterns → query executing inside a loop
- Large result sets without pagination
- Missing `WHERE` clause on large tables
- Unnecessary JOINs that could be a separate dedicated query

Format:
> 💡 Optimization suggestion: `SELECT *` found in `GetAllAsync` — recommend explicit columns to reduce payload. Approve to apply?
