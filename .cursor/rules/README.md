# Cursor Rules Index

Supported Cursor entrypoint and read-order index for this repository.

## Always-On Rules

- [`00-entrypoint.mdc`](00-entrypoint.mdc) — read order and source links
- [`pattern-match.mdc`](pattern-match.mdc) — template-first generation
- [`context-discovery.mdc`](context-discovery.mdc) — resolve naming and placement from docs first

## C# Scoped Rules

- [`rule-guard.mdc`](rule-guard.mdc) — audit layering and runtime rules
- [`shadow-ref.mdc`](shadow-ref.mdc) — use shadow examples for complex shapes
- [`skeleton-sync.mdc`](skeleton-sync.mdc) — keep DI and composition updates aligned

## Manual Workflow Rules

- [`refactor-uow.mdc`](refactor-uow.mdc) — transaction boundaries and UoW flows
- [`add-resilience.mdc`](add-resilience.mdc) — timeout, retry, breaker, outbound safeguards
- [`api-standard.mdc`](api-standard.mdc) — thin controllers and API response handling

## Use Order

1. Start with [`00-entrypoint.mdc`](00-entrypoint.mdc).
2. Use [`pattern-match.mdc`](pattern-match.mdc) before inventing structures.
3. Run [`rule-guard.mdc`](rule-guard.mdc) after backend C# changes.
4. Load a manual workflow rule when the request or audit matches.

## Maintenance

- Keep these rules aligned with `docs/ARCHITECTURE.md` and `docs/rules/*.md`.
- Treat `.cursor/rules/` as the stable, versioned, and only supported Cursor entrypoint and read-order index for this repository.
- Treat `docs/ARCHITECTURE.md`, `docs/rules/*.md`, `templates/`, and `skeleton/` as the repository source of truth.
- Prefer editing a focused rule instead of expanding `00-entrypoint.mdc` into a monolith.
