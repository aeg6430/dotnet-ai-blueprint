# Project Architecture & Coding Standards

> **Placeholders:** `{Solution}`, `{CoreNamespace}`, `{InfrastructureNamespace}`, `{ApiNamespace}`, `{TestsNamespace}` — replace using the project setup flow described in [`docs/starter-pack/README.md`](starter-pack/README.md) and the repository root [`README.md`](../README.md). **`{BackendRoot}`** is the directory that contains your backend `.csproj` trees (often `src` or `src/backend`). Project folder names frequently mirror namespaces (e.g. `Acme.Core`).
>
> **Audience:** This document describes a **typical layered .NET backend** aligned with this pack. Feature specs under `docs/specs/` (if present in your repo) override silent areas here. For Phase A–E onboarding, see [`README.md`](../README.md) at repository root, or start from [`docs/starter-pack/README.md`](starter-pack/README.md) alone.

## 0. Spec Override — Global Rule

- Spec defines it → follow the spec
- Spec is silent → fall back to this document
- Spec conflicts with hard rule (SOLID, no magic numbers) → flag and ask
- Spec is ambiguous → flag as blind spot, ask before writing code

### 0.1 Typical repository layout (examples)

| Area | Example location | Notes |
|------|------------------|--------|
| Backend projects | `{BackendRoot}/{ApiNamespace}/`, `{CoreNamespace}/`, `{InfrastructureNamespace}/`, `{TestsNamespace}/` | ASP.NET Core + Dapper + Mapperly pattern assumed |
| Optional SPAs / workers | `src/frontend/`, `workers/`, … | Not defined by this pack |
| Product specs | `docs/specs/` | Optional per repo |

### 0.2 Backend layer boundaries

**Dependency rule:** **`{CoreNamespace}`** must **not** reference **`{InfrastructureNamespace}`** or **`{ApiNamespace}`**. The API references both Core and Infrastructure for DI composition.

- **`{ApiNamespace}`** — HTTP surface: `Controllers/`, `Filters/`, `Models/` (DTOs), `Extensions/` (including DI composition), `Middlewares/`, `Startup.cs` / `Program.cs`. Keep controllers thin; orchestration belongs in Core services.

- **`{CoreNamespace}`** — Domain and use cases: `Services/`, `DTOs/`, `Interfaces/`, validation/analysis folders as needed. **No** direct SQL, **no** persistence provider types (`Npgsql.*`, `Dapper.*`, etc.).

- **`{InfrastructureNamespace}`** — Persistence and adapters: `Repositories/` (typically inherit shared `BaseRepository`), `Context/`, `Mappers/` (Mapperly projections), `Entities/`, `Persistence/`, `JWT/`, `Lookups/`, `TypeHandlers/`, `Security/`. Implements interfaces declared in Core.

### 0.3 Pattern cookbook (`templates/`)

After export, generic snippets live under **`templates/`** (fictional `Project.*` / `Warehouse*` / `StockService` names). Use them as **copy sources**, then rename to `{CoreNamespace}` / `{InfrastructureNamespace}` / `{ApiNamespace}` conventions.

### 0.4 Automated architecture checks (Phase 1–6)

Encode layering and firewall rules as **tests** so refactors do not rely on informal review alone.

- **Canonical rule IDs:** [`docs/rules/architecture-protocol.md`](rules/architecture-protocol.md) → §8
- **Phase 1 + Phase 4 (layering / composition):** `{BackendRoot}/{TestsNamespace}/Architecture/LayeringArchitectureTests.cs` (from [`docs/starter-pack/architecture-tests/`](starter-pack/architecture-tests/))
- **Phase 2–3 + Phase 5 (repository / adjunct firewalls):** `{BackendRoot}/{TestsNamespace}/Architecture/RepositoryFirewallArchitectureTests.cs`
- **Phase 6 (service + API firewalls):** `ServiceFirewallArchitectureTests.cs`, `ApiFirewallArchitectureTests.cs` alongside the above

---

## 1. Directory Structure (example)

Folders inside Core/API/Infrastructure vary by domain; below is a **common** skeleton:

```
{BackendRoot}/
├── {ApiNamespace}/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Filters/
│   ├── Middlewares/
│   ├── Models/
│   └── …
├── {CoreNamespace}/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   └── …
├── {InfrastructureNamespace}/
│   ├── Context/
│   ├── Entities/
│   ├── Helpers/
│   ├── Lookups/
│   ├── Mappers/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Security/
│   └── TypeHandlers/
└── {TestsNamespace}/
    └── Architecture/           ← firewall / layering tests (recommended)

docs/
├── ARCHITECTURE.md
├── rules/
└── …                          ← specs/diagrams optional per repo

templates/                     ← pattern cookbook (not necessarily part of the built solution)
├── IDapperContext.cs
├── DapperContext.cs
├── BaseHttpAdapter.cs
├── IInventoryGateway.cs
├── IPaymentGateway.cs
├── IPricingGateway.cs
├── IShipmentGateway.cs
├── IWebhookGateway.cs
├── IMessagePublisher.cs
├── ITransferIdempotencyRepository.cs
├── IOutboxRepository.cs
├── InventoryGatewayOptions.cs
├── PaymentGatewayOptions.cs
├── PricingGatewayOptions.cs
├── ShipmentGatewayOptions.cs
├── WebhookGatewayOptions.cs
├── MessagePublisherOptions.cs
├── OutboxDeliveryOptions.cs
├── InventoryGateway.cs
├── PaymentGateway.cs
├── PricingGateway.cs
├── ShipmentGateway.cs
├── WebhookGateway.cs
├── MessagePublisher.cs
├── InventoryVerificationRequest.cs
├── InventoryVerificationResponse.cs
├── PaymentAuthorizeRequest.cs
├── PaymentAuthorizeResponse.cs
├── PricingQuoteRequest.cs
├── PricingQuoteResponse.cs
├── ShipmentCreateRequest.cs
├── ShipmentCreateResponse.cs
├── WebhookDeliveryRequest.cs
├── WebhookDeliveryResponse.cs
├── MessagePublishRequest.cs
├── BaseRepository.cs
├── IOutboxMessageSerializer.cs
├── OutboxEnvelope.cs
├── OutboxMessageRecord.cs
├── OutboxMessageSerializer.cs
├── IOutboxDispatcher.cs
├── IOutboxDeliveryHandler.cs
├── OutboxDispatcher.cs
├── OutboxDeliveryWorker.cs
├── PaymentOutboxDeliveryHandler.cs
├── WebhookOutboxDeliveryHandler.cs
├── MessagePublishOutboxDeliveryHandler.cs
├── IStockLedgerRepository.cs
├── StockLedgerRepository.cs
├── TransferIdempotencyRepository.cs
├── OutboxRepository.cs
├── WarehouseRepository.cs
├── StockService.cs
├── StockTransferDto.cs
├── StockTransferCommitted.cs
├── StockTransferService.cs
├── StockTransferUseCase.cs
├── ResiliencePolicies.cs
├── WarehouseMapper.cs
├── GlobalExceptionHandler.cs
├── ServiceExtensions.cs
├── ApiResponse.cs
└── …
```

---

## 2. Database & Transaction Pattern

- **`IDapperContext`** (or equivalent UoW) — see `{BackendRoot}/{InfrastructureNamespace}/Context/` and [`templates/`](../templates/) for patterns
- **Scoped** context per request/use case; repositories do not own connections or transactions
- **Lazy connection opening:** opening the DB connection is deferred until the first real SQL call
- **Default transaction model:** explicit, short-lived transaction around the smallest local atomic DB write block
- **Remote IO rule:** no external API/network call may occur while the main DB transaction is active
- **Read-only rule:** query/lookup paths do not call `Begin()`
- Binding detail lives in [`docs/rules/transactions.md`](rules/transactions.md); runtime timeout/retry/circuit-breaker policy lives in [`docs/rules/resilience.md`](rules/resilience.md)

---

## 3. BaseRepository

Repositories inherit `BaseRepository` in Infrastructure. Never create ad hoc connections per repository method.

See **`templates/BaseRepository.cs`** for an isolated illustration; implement **`BaseRepository`** in `{InfrastructureNamespace}` for real code.

---

## 4. Service layer & use-case orchestration

Services orchestrate use cases and validation. When a feature needs a transaction, the service/use-case opens it **explicitly and late**, keeps it short, and closes it immediately after the local atomic write block.

- Pure read/query flows do not start transactions.
- External API calls happen **before** entering the transaction, or after commit via outbox/background work.
- Use cases that mix local writes and cross-system side effects should prefer outbox over direct remote calls inside the main transaction.

See **`templates/StockService.cs`** and **`templates/StockTransferUseCase.cs`** for the recommended orchestration shapes, plus **`templates/InventoryGateway.cs`**, **`templates/PricingGateway.cs`**, **`templates/ShipmentGateway.cs`**, **`templates/PaymentGateway.cs`**, **`templates/WebhookGateway.cs`**, and **`templates/MessagePublisher.cs`** for outbound adapter variants. Repository implementations for the outbox/idempotency path live in **`templates/OutboxRepository.cs`** and **`templates/TransferIdempotencyRepository.cs`**, while **`templates/OutboxDispatcher.cs`** and **`templates/OutboxDeliveryWorker.cs`** show the post-commit delivery path for webhook/broker/payment side effects.

---

## 5. Unified API Response

Many APIs standardize on **`ApiResponse<T>`**.

See **`templates/ApiResponse.cs`** and align `{ApiNamespace}` controllers with your adopted contract.

---

## 6. Object Mapping

**Riok.Mapperly** only for declared maps. `partial` classes with `[Mapper]` typically live under **`{InfrastructureNamespace}/Mappers/`** (persistence-facing) and **`{ApiNamespace}/Mappers/`** (API-specific).

Never map manually inside repositories or services beyond thin delegation; entities do not leak above Infrastructure.

See **`templates/WarehouseMapper.cs`** for extra examples.

---

## 7. Global Error Handling

Implement **`IExceptionHandler`** (or equivalent) in the API layer so clients receive safe **`ProblemDetails`** while full exceptions are logged.

For audit-sensitive environments, keep the API entry point and exception boundary aligned with [`docs/rules/audit-log.md`](rules/audit-log.md) so audit events are captured consistently with actor and correlation context.

See **`templates/GlobalExceptionHandler.cs`**.

---

## 8. Pagination Strategy

| Scenario | Strategy | Fields |
|---|---|---|
| High-traffic / Mobile | Cursor-based | `NextCursor`, `Limit` |
| Admin / Back-office | Offset-based | `Page`, `PageSize`, `TotalCount` |

Spec silent but feature needs pagination → flag as blind spot, ask first.

---

## 9. DI Registration

Prefer **`{ApiNamespace}/Extensions/ServiceExtensions.cs`** (or grouped extension methods) over stuffing unrelated registrations into **`Program.cs`**.

See **`templates/ServiceExtensions.cs`**.

---

## 10. Implementation Rules

| Rule | Detail |
|---|---|
| ORM | Dapper only. EF Core forbidden in repositories unless an explicit ADR allows an exception. |
| Async | All I/O must be `async/await` |
| SQL Safety | Parameterized only — no string interpolation |
| Transactions | Explicit short-lived UoW at the use-case boundary; no remote IO inside the active DB transaction; see [`docs/rules/transactions.md`](rules/transactions.md) |
| Logging | `ILogger<T>` in Services and Repositories; log full exception in `catch`; keep API-edge audit capture aligned with [`docs/rules/audit-log.md`](rules/audit-log.md) |
| Naming | Prefer descriptive field names (e.g. `_warehouseRepository` over vague abbreviations) |
| DI | Extension registration classes under `{ApiNamespace}/Extensions/` |
| Entities | Never expose Infrastructure entities above Infrastructure |
| Mapping | Mapperly only |
| Testing | Typical: **NUnit** + **Moq**; mock `IDapperContext` / repositories in unit tests |
