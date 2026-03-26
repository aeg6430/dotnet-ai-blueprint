---
inclusion: fileMatch
fileMatchPattern: "src/**/Mappers/**/*.cs"
---

# Mapping Rules

## Tool
Use **Riok.Mapperly** exclusively. No AutoMapper, no manual mapping in Services/Repositories.
Docs: https://mapperly.riok.app/docs/intro/

## Definition
- `partial` classes with `[Mapper]` attribute
- Location: `Project.Core/Mappers/` — one mapper per domain entity

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
