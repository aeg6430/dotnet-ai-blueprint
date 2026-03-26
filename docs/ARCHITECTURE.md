# Project Architecture & Coding Standards

## 0. Spec Override — Global Rule
- Spec defines it → follow the spec
- Spec is silent → fall back to this document
- Spec conflicts with hard rule (SOLID, no magic numbers) → flag and ask
- Spec is ambiguous → flag as blind spot, ask before writing code

---

## 1. Directory Structure

```
docs/
├── ARCHITECTURE.md
├── rules/                    ← read on demand (sql, mapping, code-quality, testing, review-learning, not-implemented-pattern)
├── specs/                    ← one file per feature; always override defaults
└── diagrams/

templates/                    ← canonical patterns — always read during bootstrap
├── BaseRepository.cs
├── WarehouseRepository.cs    ← 3 parameter cases + logging
├── StockService.cs           ← transaction orchestration
├── WarehouseMapper.cs        ← 5 Mapperly cases
├── GlobalExceptionHandler.cs
├── ServiceExtensions.cs
├── ApiResponse.cs
├── NotImplementedPattern.cs
├── PaginationTemplate.cs
├── Program.cs
├── Startup.cs
├── appsettings.json / appsettings.Development.json
├── nlog.config
└── Project.Api.http

src/
├── Project.Api/
│   ├── Extensions/ServiceExtensions.cs   ← ALL DI here, never Program.cs
│   ├── Middlewares/GlobalExceptionHandler.cs
│   ├── Controllers/                       ← routing + validation only
│   ├── Models/                            ← Request/Response (API contract only)
│   └── Program.cs
├── Project.Core/
│   ├── DTOs/                              ← internal transfer only
│   ├── Interfaces/IServices/ + IRepositories/
│   ├── Enums/
│   ├── Mappers/                           ← Mapperly partial classes
│   ├── Models/
│   └── Services/                          ← business logic + transaction control
├── Project.Infrastructure/
│   ├── Context/               ← IDatabaseFactory, IDapperContext, DapperContext, DatabaseFactory
│   ├── Repositories/          ← Dapper, all inherit BaseRepository
│   └── Entities/              ← never exposed above Infrastructure
└── Project.Tests/ServiceTests/
```

---

## 2. Database & Transaction Pattern

- `IDatabaseFactory` — encapsulates connection string; see `templates/`
- `IDapperContext` — Unit of Work contract: `Connection`, `Transaction`, `Begin()`, `Commit()`, `Rollback()`
- `DapperContext` — registered as **Scoped**; lazily opens connection; all repos in one request share same instance
- Only Services call `Begin/Commit/Rollback` — Repositories are transaction-unaware

---

## 3. BaseRepository

All repositories inherit `BaseRepository`. Never create their own connection.  
See `templates/BaseRepository.cs` and `templates/WarehouseRepository.cs`.

---

## 4. Service Layer & Transaction Orchestration

Service is the only layer that controls transaction boundaries.  
See `templates/StockService.cs`.

---

## 5. Unified API Response

All endpoints return `ApiResponse<T>`.  
See `templates/ApiResponse.cs`.

---

## 6. Object Mapping

Riok.Mapperly only. `partial` classes with `[Mapper]` in `Project.Core/Mappers/`.  
Never map manually in Services or Repositories.  
Entities never exposed above Infrastructure — always map to DTO first.  
See `templates/WarehouseMapper.cs`.

---

## 7. Global Error Handling

Implement `IExceptionHandler` in `Api/Middlewares/`.  
See `templates/GlobalExceptionHandler.cs`.

---

## 8. Pagination Strategy

| Scenario | Strategy | Fields |
|---|---|---|
| High-traffic / Mobile | Cursor-based | `NextCursor`, `Limit` |
| Admin / Back-office | Offset-based | `Page`, `PageSize`, `TotalCount` |

Spec silent but feature needs pagination → flag as blind spot, ask first.

---

## 9. DI Registration

All registrations in `Api/Extensions/ServiceExtensions.cs`. Never `Program.cs`.  
See `templates/ServiceExtensions.cs`.

---

## 10. Implementation Rules

| Rule | Detail |
|---|---|
| ORM | Dapper only. EF Core forbidden. |
| Async | All I/O must be `async/await` |
| SQL Safety | Parameterized only — no string interpolation |
| Transactions | Only Services call Begin/Commit/Rollback |
| Logging | `ILogger<T>` in Services and Repositories. Log full exception in `catch`. |
| Naming | Full names: `_warehouseRepository` never `_warehouseRepo` |
| DI | `ServiceExtensions.cs` only |
| Entities | Never expose above Infrastructure |
| Mapping | Mapperly only |
| Testing | NUnit + Moq; mock `IDapperContext` and repos |
