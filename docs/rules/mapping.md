# Mapping Rules

> Read this file when working on Mapperly mappers, DTOs, or API models.
> See `templates/WarehouseMapper.cs` for code examples of all cases below.

---

## Tool

Use **Riok.Mapperly** exclusively. No AutoMapper, no manual mapping in Services or Repositories.

Reference docs (read when template doesn't cover your scenario):
https://mapperly.riok.app/docs/intro/

---

## Definition

- Mappers are `partial` classes decorated with `[Mapper]`
- Location: `Project.Core/Mappers/`
- One mapper class per domain entity (e.g. `WarehouseMapper`, `StockMapper`)

---

## Layer Boundaries

| From | To | Where mapped |
|---|---|---|
| `Infrastructure/Entities` | `Core/DTOs` | Service or Repository layer |
| `Core/DTOs` | `Api/Models` (Response) | Service or Controller layer |
| `Api/Models` (Request) | `Core/DTOs` | Controller or Service layer |

- Entities must never be exposed above the Infrastructure layer
- DTOs must never be returned directly from Controllers — always map to a Response model first
- Never reuse a DTO as an API model or vice versa

---

## Cases

Five cases are covered in `templates/WarehouseMapper.cs`:

1. **Flat mapping** — property names match exactly
2. **List mapping** — always declare explicitly, Mapperly does not generate this automatically
3. **Property name mismatch** — use `[MapProperty(nameof(Source.Prop), nameof(Target.Prop))]`
4. **DTO → API Response model** — internal DTO to API contract
5. **API Request model → DTO** — incoming request to internal DTO

If your scenario is not covered by the template → consult the Mapperly docs before implementing.
