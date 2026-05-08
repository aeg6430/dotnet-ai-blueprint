---
inclusion: fileMatch
fileMatchPattern: "src/**/Mappers/**/*.cs"
---

# Mapping Rules

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: do-not-touch existing mappers if they work. Apply mapping rules to new DTOs/endpoints first.
- **Strict (new project)**: enforce mapping boundaries and naming conventions globally.

## Tool
Use **Riok.Mapperly** exclusively. No AutoMapper, no manual mapping in Services/Repositories.
Docs: https://mapperly.riok.app/docs/intro/

## Definition
- `partial` classes with `[Mapper]` attribute
- Location: **`{InfrastructureNamespace}/Mappers/`** for persistence-facing maps, **`{ApiNamespace}/Mappers/`** for API-specific maps — see [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md) §6. Cookbook files under `templates/` may use fictional namespaces such as `Project.Core.Mappers` for illustration only.

Bad (stop):

```csharp
// service-level manual mapping (drifts over time)
var dto = new UserDto
{
    Id = entity.Id,
    Email = entity.Email,
    // ... dozens of fields ...
};
```

Good (follow):

```csharp
[Mapper]
public partial class UserMapper
{
    public partial UserDto ToDto(UserEntity entity);
}
```

## Layer Boundaries
- `Infrastructure/Entities` → `Core/DTOs`: Service or Repository layer
- `Core/DTOs` → `Api/Models` (Response): Service or Controller layer
- `Api/Models` (Request) → `Core/DTOs`: Controller or Service layer
- Entities never exposed above Infrastructure
- DTOs never returned directly from Controllers — always map to Response model
- Never reuse DTO as API model or vice versa

## Cases (see `templates/WarehouseMapper.cs`)
1. Flat mapping — exact property name match
2. List mapping — always declare explicitly
3. Name mismatch — `[MapProperty(nameof(Source.Prop), nameof(Target.Prop))]`
4. DTO → API Response model
5. API Request model → DTO

Scenario not in template → consult Mapperly docs before implementing.
