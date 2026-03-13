# Project Architecture & Coding Standards

---

## 0. Spec Override — Global Rule

**Specs in `docs/specs/` always take priority over any default rule in this document — no exceptions.**

- If a spec defines something → follow the spec
- If a spec is silent on something → fall back to the defaults in this document
- If a spec contradicts a hard rule (SOLID, no magic numbers, etc.) → flag the conflict to the user and ask before proceeding
- If the spec is ambiguous → flag as a blind spot during bootstrap and ask for clarification before writing any code

---

## 1. Directory Structure

```text
docs/
├── ARCHITECTURE.md               (This file — architecture rules and standards)
├── rules/                        (Detailed rule files — read on demand by AI IDE and Copilot Chat)
│   ├── sql.md                    (SQL ownership, parameter declaration, ORDER BY, batch, optimization)
│   ├── mapping.md                (Mapperly patterns, DTO ↔ API model boundaries)
│   ├── code-quality.md           (Fowler smells, async rules, nesting, null safety)
│   ├── testing.md                (NUnit + Moq patterns, naming, coverage requirements)
│   ├── review-learning.md        (Code Review Mode, Learning Mode, Blind Spot Mode)
│   └── not-implemented-pattern.md(How to handle code the AI or developer cannot implement)
├── specs/                        (Feature specifications — one file per feature)
│   ├── feature-spec-template.md  (Copy this when starting a new feature)
│   └── {feature-name}.md
└── diagrams/                     (Visual references — ER diagrams, system overview, flow diagrams)
    └── {diagram-name}.png

templates/                        (Canonical code patterns — AI always reads these during bootstrap)
├── BaseRepository.cs             (Abstract base — all repositories inherit this)
├── WarehouseRepository.cs        (Dapper repository — 3 parameter cases + logging pattern)
├── StockService.cs               (Service with transaction orchestration pattern)
├── WarehouseMapper.cs            (Mapperly — 5 mapping cases)
├── GlobalExceptionHandler.cs     (IExceptionHandler implementation)
├── ServiceExtensions.cs          (DI registration — copy and extend per project)
├── ApiResponse.cs                (Unified API response wrapper)
├── NotImplementedPattern.cs      (How to stub unimplemented code safely)
├── PaginationTemplate.cs         (Offset-based and cursor-based pagination patterns)
├── Program.cs                    (Lean entry point with NLog setup)
├── Startup.cs                    (JWT, Swagger, CORS, middleware pipeline)
├── appsettings.json              (Base config — no secrets)
├── appsettings.Development.json  (Dev config — placeholder values only)
├── nlog.config                   (NLog — console + file + error file targets)
└── Project.Api.http              (HTTP request file placeholder)

src/
├── Project.Api/                    (Entry Point & Configuration)
│   ├── Extensions/
│   │   └── ServiceExtensions.cs   <-- ALL Dependency Injection (DI) logic lives here
│   ├── Middlewares/
│   │   └── GlobalExceptionHandler.cs  <-- Centralized error handling (IExceptionHandler)
│   ├── Controllers/               (API Endpoints - No business logic)
│   ├── Models/                    (API Request / Response models only — not DTOs)
│   └── Program.cs                 (Lean Entry Point - Calls ServiceExtensions)
│
├── Project.Core/                  (Domain & Business Logic)
│   ├── DTOs/                      (Internal Data Transfer Objects between layers only)
│   ├── Interfaces/                (Contracts — split by type)
│   │   ├── IServices/             (e.g. IStockService, IWarehouseService)
│   │   └── IRepositories/         (e.g. IWarehouseRepository, IAuditRepository)
│   ├── Enums/                     (Shared enum definitions)
│   ├── Mappers/                   (Mapperly partial classes)
│   ├── Models/                    (Domain Models)
│   └── Services/                  (Business Workflows - Transaction control starts here)
│
├── Project.Infrastructure/
│   ├── Context/
│   │   ├── IDatabaseFactory.cs    <-- Interface: responsible for creating IDbConnection
│   │   ├── IDapperContext.cs      <-- Interface: Unit of Work contract
│   │   ├── DatabaseFactory.cs     <-- Concrete SQL Server implementation
│   │   └── DapperContext.cs       <-- Concrete: connection + transaction lifecycle
│   ├── Repositories/              (Dapper SQL implementations - all inherit BaseRepository)
│   └── Entities/                  (Database Table Models - never exposed above Infrastructure)
│
└── Project.Tests/
    └── ServiceTests/              (NUnit + Moq - repository layer is fully mocked)
```

---

## 2. Database & Transaction Pattern

We use a **Factory-driven Context** to manage Dapper connections and transactions.

### IDatabaseFactory (`Infrastructure/Context`)

Encapsulates the connection string. Swap providers (SQL Server → PostgreSQL) or inject a mock without touching business logic.

```csharp
public interface IDatabaseFactory
{
    IDbConnection CreateConnection();
}
```

### IDapperContext (`Infrastructure/Context`)

Defines the Unit of Work contract used by both Services and Repositories. Lives alongside its implementation in the Context folder.

```csharp
public interface IDapperContext : IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    void Begin();
    void Commit();
    void Rollback();
}
```

### DapperContext (`Infrastructure/Context`)

Implements `IDapperContext`. Lazily opens the connection and manages the transaction lifecycle. Registered as **Scoped** so all repositories within a single request share the same instance.

```csharp
public class DapperContext : IDapperContext
{
    private readonly IDatabaseFactory _factory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public DapperContext(IDatabaseFactory factory) => _factory = factory;

    public IDbConnection Connection => _connection ??= _factory.CreateConnection();
    public IDbTransaction? Transaction => _transaction;

    public void Begin()
    {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        _transaction = Connection.BeginTransaction();
    }

    public void Commit()   { _transaction?.Commit();   Dispose(); }
    public void Rollback() { _transaction?.Rollback(); Dispose(); }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
        _connection = null;
        _transaction = null;
    }
}
```

---

## 3. BaseRepository (`Infrastructure/Repositories`)

All repositories **must** inherit `BaseRepository`. This ensures they always access the connection and transaction that are managed by `DapperContext` at the Service layer — never creating their own.

```csharp
public abstract class BaseRepository
{
    protected readonly IDapperContext _context;

    protected BaseRepository(IDapperContext context) => _context = context;

    protected IDbConnection Connection   => _context.Connection;
    protected IDbTransaction? Transaction => _context.Transaction;
}
```

**Concrete repository pattern:**

```csharp
public class WarehouseRepository : BaseRepository, IWarehouseRepository
{
    private readonly ILogger<WarehouseRepository> _logger;

    public WarehouseRepository(IDapperContext context, ILogger<WarehouseRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    // Case 1: Manual parameters — declare var parameters separately
    public async Task UpdateStockAsync(int skuId, int quantity)
    {
        const string sql = @"
            UPDATE Inventory SET Stock = Stock - @Quantity
            WHERE SkuId = @SkuId";

        var parameters = new
        {
            SkuId    = skuId,
            Quantity = quantity
        };

        try
        {
            _logger.LogDebug("Executing UpdateStockAsync for SkuId: {SkuId}", skuId);
            await Connection.ExecuteAsync(sql, parameters, Transaction);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating stock for SkuId: {SkuId}", skuId);
            throw;
        }
    }

    // Case 2: Whole entity — pass directly, no wrapper needed
    public async Task InsertWarehouseAsync(WarehouseEntity entity)
    {
        const string sql = @"
            INSERT INTO Warehouse (Name, Location, Capacity)
            VALUES (@Name, @Location, @Capacity)";

        try
        {
            _logger.LogDebug("Executing InsertWarehouseAsync for Warehouse: {Name}", entity.Name);
            await Connection.ExecuteAsync(sql, entity, Transaction);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inserting warehouse: {Name}", entity.Name);
            throw;
        }
    }

    // Case 3: ORDER BY direction — cannot be parameterized, use whitelist interpolation
    public async Task<IEnumerable<WarehouseEntity>> GetAllAsync(bool isDescending = false)
    {
        var direction = isDescending ? "DESC" : "ASC";

        var sql = $@"
            SELECT Id, Name, Location, Capacity
            FROM Warehouse
            ORDER BY Name {direction}";

        try
        {
            _logger.LogDebug("Executing GetAllAsync with direction: {Direction}", direction);
            return await Connection.QueryAsync<WarehouseEntity>(sql, transaction: Transaction);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving warehouses");
            throw;
        }
    }
}
```

---

## 4. Service Layer & Transaction Orchestration

The **Service** is the only layer that calls `Begin()`, `Commit()`, and `Rollback()`. Repositories are unaware of transaction boundaries.

```csharp
public class StockService : IStockService
{
    private readonly IDapperContext _context;
    private readonly ILogger<StockService> _logger;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IAuditRepository _auditRepository;

    public StockService(
        IDapperContext context,
        ILogger<StockService> logger,
        IWarehouseRepository warehouseRepository,
        IAuditRepository auditRepository)
    {
        _context = context;
        _logger = logger;
        _warehouseRepository = warehouseRepository;
        _auditRepository = auditRepository;
    }

    public async Task TransferStockAsync(StockTransferDto dto)
    {
        _logger.LogInformation("Starting stock transfer for SkuId: {SkuId}", dto.SkuId);
        _context.Begin();

        try
        {
            await _warehouseRepository.UpdateStockAsync(dto.SkuId, dto.Quantity);
            await _auditRepository.LogChangeAsync($"Transferred {dto.Quantity} units");
            _context.Commit();
            _logger.LogInformation("Stock transfer committed for SkuId: {SkuId}", dto.SkuId);
        }
        catch (Exception e)
        {
            _context.Rollback();
            _logger.LogError(e, "Stock transfer failed for SkuId: {SkuId}. Rolled back.", dto.SkuId);
            throw;
        }
    }
}
```

---

## 5. Unified API Response (`Api/Models`)

All endpoints return `ApiResponse<T>` to guarantee a consistent JSON contract.

```csharp
public class ApiResponse<T>
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static ApiResponse<T> Success(T data, string message = "Success", int statusCode = 200)
        => new() { StatusCode = statusCode, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
        => new() { StatusCode = statusCode, Message = message };
}
```

**Controller example:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetWarehouse(int id)
{
    var result = await _warehouseService.GetByIdAsync(id);
    return Ok(ApiResponse<WarehouseDto>.Success(result));
}
```

---

## 6. Object Mapping (`Core/Mappers`)

Use **Riok.Mapperly** exclusively. Define mappers as `partial` classes decorated with `[Mapper]`.

```csharp
[Mapper]
public partial class WarehouseMapper
{
    public partial WarehouseDto EntityToDto(WarehouseEntity entity);
}
```

- Never map manually inside Services or Repositories.
- Entities (`Infrastructure/Entities`) must never be exposed above the Infrastructure boundary — always map to a DTO before returning from a Repository if needed, or map in the Service.
- Mapperly can also be used to map between DTOs (`Core/DTOs/`) and API models (`Api/Models/`) when needed — define these in `Project.Core/Mappers/` following the same pattern.

---

## 7. Global Error Handling (`Api/Middlewares`)

Implement `IExceptionHandler` for centralized, structured error responses.

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException      => (401, "Unauthorized"),
            KeyNotFoundException             => (404, "Not Found"),
            ArgumentException
            or InvalidOperationException     => (400, "Bad Request"),
            _                                => (500, "Internal Server Error")
        };

        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled Exception: {Title} - {Message}", title, exception.Message);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = exception.Message,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
```

---

## 8. Pagination Strategy


| Scenario | Strategy | Key Fields |
|---|---|---|
| High-traffic / Mobile | Cursor-based | `NextCursor`, `Limit` |
| Admin / Back-office | Offset-based | `Page`, `PageSize`, `TotalCount` |

If the spec is ambiguous or silent on pagination but the feature clearly needs it, flag as a blind spot and ask before implementing (see Section 0).

---

## 9. DI Registration (`Api/Extensions/ServiceExtensions.cs`)

**All** registrations live here. Never register directly in `Program.cs`.

```csharp
public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IDatabaseFactory>(_ =>
            new DatabaseFactory(config.GetConnectionString("DefaultConnection")!));

        services.AddScoped<IDapperContext, DapperContext>();

        // Repositories
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        // Services
        services.AddScoped<IStockService, StockService>();

        // Error handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
```

---

## 10. Implementation Rules (Summary)

| Rule | Detail |
|---|---|
| **ORM** | Dapper only. EF Core is forbidden. |
| **Async** | All I/O operations must be `async/await`. |
| **SQL Safety** | Always use parameterized queries — no string interpolation in SQL. |
| **Transactions** | Only Services call `Begin/Commit/Rollback`. |
| **Logging** | Inject `ILogger<T>` in both Services and Repositories. Log the full exception object in `catch`. |
| **Naming** | Full names only: `_warehouseRepository`, never `_warehouseRepo`. |
| **DI** | `ServiceExtensions.cs` only. Never `Program.cs`. |
| **Entities** | Never expose Infrastructure Entities above the Infrastructure layer. |
| **Mapping** | Mapperly only. No manual mapping in Services. |
| **Testing** | NUnit + Moq. Mock `IDapperContext` and repositories to test Services in isolation. |
