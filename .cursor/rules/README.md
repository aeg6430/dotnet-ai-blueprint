# Cursor Rules Index

Cursor-specific rule index for this repository.

## Start Here

1. Start with [`00-entrypoint.mdc`](00-entrypoint.mdc).
2. Use [`pattern-match.mdc`](pattern-match.mdc) before inventing structures.
3. Use [`context-discovery.mdc`](context-discovery.mdc) when placement, naming, or ownership is unclear.

## Always-On Rules

- [`00-entrypoint.mdc`](00-entrypoint.mdc) — shared read order and source links
- [`pattern-match.mdc`](pattern-match.mdc) — template-first generation
- [`context-discovery.mdc`](context-discovery.mdc) — resolve naming and placement from docs first

## Common Supporting Rules

- [`rule-guard.mdc`](rule-guard.mdc) — layering and runtime rule checks
- [`shadow-ref.mdc`](shadow-ref.mdc) — shadow examples for complex shapes
- [`skeleton-sync.mdc`](skeleton-sync.mdc) — keep DI and composition updates aligned

## Task-Specific Rules

- [`refactor-uow.mdc`](refactor-uow.mdc) — transaction boundaries and UoW flows
- [`add-resilience.mdc`](add-resilience.mdc) — timeout, retry, breaker, outbound safeguards
- [`api-standard.mdc`](api-standard.mdc) — thin controllers and API response handling
- [`legacy-project-rule.mdc`](legacy-project-rule.mdc) — prevent transaction-model mixing and sync-over-async regressions in legacy edits
- [`seed-to-legacy-target-translation.mdc`](seed-to-legacy-target-translation.mdc) — translate Seed logic into an existing Target repo safely
- [`seed-to-new-project-target-translation.mdc`](seed-to-new-project-target-translation.mdc) — translate Seed logic into a new Target repo with target-native structure

Load task-specific rules when the request matches:

- existing legacy target repo -> [`legacy-project-rule.mdc`](legacy-project-rule.mdc)
- existing target repo translation -> [`seed-to-legacy-target-translation.mdc`](seed-to-legacy-target-translation.mdc)
- new or early-stage target repo translation -> [`seed-to-new-project-target-translation.mdc`](seed-to-new-project-target-translation.mdc)

## Project Setup Protocol

The canonical project setup flow lives in [`../../docs/starter-pack/project-setup-protocol.md`](../../docs/starter-pack/project-setup-protocol.md).

Use that shared document when a Seed folder or exported starter pack needs to be renamed and aligned to a target project before implementation work begins.

When setup is being automated, prefer the reviewed `Makefile` setup targets rather than composing ad hoc rename or cleanup commands. The intended sequence is `setup-scan`, `setup-rewrite-placeholders`, `setup-rewrite-content`, `setup-rename-solution-projects`, `setup-rename-paths`, `setup-clean`, and `setup-verify`.

After setup, continue from [`00-entrypoint.mdc`](00-entrypoint.mdc) and then load the appropriate translation rule for the destination repository.

## Keep Context Focused

- Start from [`00-entrypoint.mdc`](00-entrypoint.mdc) and only load deeper rule files when the task needs them.
- Prefer focused rule files over loading the whole repository at once.
- Treat `docs/ARCHITECTURE.md`, `docs/rules/*.md`, `templates/`, and `skeleton/` as the main source of truth.
