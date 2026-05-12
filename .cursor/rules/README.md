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
4. Load a manual workflow rule when the request or audit matches:
   - existing Target repo -> [`seed-to-legacy-target-translation.mdc`](seed-to-legacy-target-translation.mdc)
   - new or early-stage Target repo -> [`seed-to-new-project-target-translation.mdc`](seed-to-new-project-target-translation.mdc)

## Project Setup Protocol

Use this protocol when a Seed folder or exported starter pack needs to be renamed and aligned to a target project before implementation work begins.

Suggested user prompt:

```text
Read the `Project Setup Protocol` in `.cursor/rules/README.md`.
In this directory, convert namespaces and file paths from the current Blueprint / Seed shape to `TargetProjectName`.
Also update project names, solution names, and setup-related references that carry project identity.
Keep the architecture, rule intent, and folder responsibilities intact.
If `docs/specs/` or the target repo defines a different naming scheme, follow that instead of the default.
```

Protocol:

1. Read [`00-entrypoint.mdc`](00-entrypoint.mdc), `docs/ARCHITECTURE.md`, and `docs/rules/architecture-protocol.md` first.
2. Treat `TargetProjectName` as a user-provided setup value, not a hard-coded repo name.
3. Rename namespaces, project names, solution names, directory names, and setup references that still reflect the Blueprint / Seed identity.
4. Preserve reusable rules, templates, and architecture boundaries unless the target project explicitly changes them.
5. After setup, continue with the appropriate translation rule for the destination:
   - existing Target repo -> [`seed-to-legacy-target-translation.mdc`](seed-to-legacy-target-translation.mdc)
   - new or early-stage Target repo -> [`seed-to-new-project-target-translation.mdc`](seed-to-new-project-target-translation.mdc)

## Recommended Workspace Model

These rules support both a single-repo workflow and a three-folder workflow:

- **Blueprint**: this repo; rules, templates, and source-of-truth examples.
- **Seed / Sandbox**: a clean exported project used to prototype the ideal implementation.
- **Target**: the real product repo, whether it is a legacy codebase or a new project.

When all three are present, treat the target repo's `docs/specs/` as the feature-level source of truth. Use Blueprint rules and templates to shape the design, use Seed to explore a clean implementation, and translate the logic back into Target without importing blueprint-specific abstractions unless the target repo explicitly adopts them.

## Context Loading Hygiene

Use these rules to keep AI context focused rather than loading the whole repository every time:

1. Start from [`00-entrypoint.mdc`](00-entrypoint.mdc) and only read deeper rule files when the task actually needs them.
2. Do not preload every ADR, every rule, and every template for unrelated edits; follow the read order and expand scope gradually.
3. Keep high-churn folders such as `bin/`, `obj/`, `artifacts/`, `TestResults/`, and `_starter-pack-seed/out/` out of AI indexing when possible (for example via `.cursorignore`).
4. For Phase E audits, prefer a one-shot run after feature completion or before PR submission rather than rerunning the full audit during normal editing.
5. Outside the Compliance Matrix, prefer short bullet output over long narrative reports so audit output stays reviewable.

## Maintenance

- Keep these rules aligned with `docs/ARCHITECTURE.md` and `docs/rules/*.md`.
- Treat `.cursor/rules/` as the stable, versioned, and only supported Cursor entrypoint and read-order index for this repository.
- Treat `docs/ARCHITECTURE.md`, `docs/rules/*.md`, `templates/`, and `skeleton/` as the repository source of truth.
- Prefer editing a focused rule instead of expanding `00-entrypoint.mdc` into a monolith.
