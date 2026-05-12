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
- [`seed-to-legacy-target-translation.mdc`](seed-to-legacy-target-translation.mdc) — translate Seed logic into an existing Target repo safely
- [`seed-to-new-project-target-translation.mdc`](seed-to-new-project-target-translation.mdc) — translate Seed logic into a new Target repo with target-native structure

## Use Order

1. Start with [`00-entrypoint.mdc`](00-entrypoint.mdc).
2. Use [`pattern-match.mdc`](pattern-match.mdc) before inventing structures.
3. Run [`rule-guard.mdc`](rule-guard.mdc) after backend C# changes.
4. Load a manual workflow rule when the request or audit matches.

## Recommended Workspace Model

These rules support both a single-repo workflow and a three-folder workflow:

- **Blueprint**: this repo; rules, templates, and source-of-truth examples.
- **Seed / Sandbox**: a clean exported project used to prototype the ideal implementation.
- **Target**: the real product repo, whether it is a legacy codebase or a new project.

When all three are present, treat the target repo's `docs/specs/` as the feature-level source of truth. Use Blueprint rules and templates to shape the design, use Seed to explore a clean implementation, and translate the logic back into Target without importing blueprint-specific abstractions unless the target repo explicitly adopts them.

## Maintenance

- Keep these rules aligned with `docs/ARCHITECTURE.md` and `docs/rules/*.md`.
- Treat `.cursor/rules/` as the stable, versioned, and only supported Cursor entrypoint and read-order index for this repository.
- Treat `docs/ARCHITECTURE.md`, `docs/rules/*.md`, `templates/`, and `skeleton/` as the repository source of truth.
- Prefer editing a focused rule instead of expanding `00-entrypoint.mdc` into a monolith.
