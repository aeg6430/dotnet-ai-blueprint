# Start Here

Use this page to choose the shortest repo-local starting path for common work.

If a feature spec exists under [`specs/`](specs/), follow the spec first. Use repo-local docs, templates, and examples as the primary reference for implementation work.

## Universal Read Order

Read these before substantial changes:

1. [`../README.md`](../README.md)
2. [`ARCHITECTURE.md`](ARCHITECTURE.md)
3. [`rules/architecture-protocol.md`](rules/architecture-protocol.md)
4. The relevant spec under [`specs/`](specs/), if one exists
5. Copy patterns under [`../templates/`](../templates/) and [`starter-pack/shadow-examples/`](starter-pack/shadow-examples/)

For a tool-neutral collaboration flow, also see [`../CONTRIBUTING.md`](../CONTRIBUTING.md).

## Task Routing

### Small Feature Or Bugfix In An Existing Project

Start with:

- [`starter-pack/core/daily-work-quickstart.md`](starter-pack/core/daily-work-quickstart.md)
- [`../CONTRIBUTING.md`](../CONTRIBUTING.md)
- [`starter-pack/core/legacy-bugfix-feature-sop.md`](starter-pack/core/legacy-bugfix-feature-sop.md)

Also read these when they apply:

- write path / repository change: [`rules/transactions.md`](rules/transactions.md)
- outbound or remote dependency: [`rules/resilience.md`](rules/resilience.md)
- external API or legacy integration: [`rules/external-integration-firewall.md`](rules/external-integration-firewall.md) and [`rules/anti-corruption-layer.md`](rules/anti-corruption-layer.md)

### New Project Or Day 0 Handoff

Start with:

- [`starter-pack/core/daily-work-quickstart.md`](starter-pack/core/daily-work-quickstart.md)
- [`starter-pack/core/new-project-day0-collaboration-checklist.md`](starter-pack/core/new-project-day0-collaboration-checklist.md)
- [`starter-pack/README.md`](starter-pack/README.md)

Also read these when they apply:

- setup / namespace conversion / folder rename: [`starter-pack/project-setup-protocol.md`](starter-pack/project-setup-protocol.md)
- first feature definition: [`specs/feature-spec-template.md`](specs/feature-spec-template.md) and [`specs/example-warehouse-create.md`](specs/example-warehouse-create.md)

### Project Setup / Namespace Conversion / Seed Rename

Use:

- [`starter-pack/project-setup-protocol.md`](starter-pack/project-setup-protocol.md)
- [`starter-pack/README.md`](starter-pack/README.md)

If setup is being automated, prefer the reviewed `Makefile` targets instead of synthesizing ad hoc rename or cleanup commands. Use the setup flow as small steps such as `setup-scan`, `setup-rewrite-placeholders`, `setup-rewrite-content`, `setup-rename-solution-projects`, `setup-rename-paths`, `setup-clean`, and `setup-verify`.

### Transaction, Outbound, Or Cross-System Change

Read:

- [`starter-pack/core/daily-work-quickstart.md`](starter-pack/core/daily-work-quickstart.md)
- [`rules/transactions.md`](rules/transactions.md)
- [`rules/resilience.md`](rules/resilience.md)

If the task also touches audit evidence or API-edge traceability, add:

- [`rules/audit-log.md`](rules/audit-log.md)

### Audit, Incident, Request Screening, Or Endpoint Protection

Read the matching rules directly:

- audit logging / traceability: [`rules/audit-log.md`](rules/audit-log.md)
- incident hotfix / malicious parameters / temporary path blocking: [`rules/request-screening.md`](rules/request-screening.md)
- endpoint protection / CI slowness / exception leak prevention: [`rules/endpoint-protection.md`](rules/endpoint-protection.md)

## Copy Patterns Before Inventing New Ones

Before creating a new convention, search these first:

- [`../templates/`](../templates/)
- [`starter-pack/shadow-examples/`](starter-pack/shadow-examples/)

If the target repo already has a closer local pattern, preserve the target repo's naming, style, and boundaries.

## Tool-Specific Notes

If you want tool-specific entrypoints, use:

- Cursor: [`../.cursor/rules/README.md`](../.cursor/rules/README.md)
- GitHub Copilot: [`../.github/copilot-instructions.md`](../.github/copilot-instructions.md)
- Short Copilot session bootstrap: [`../COPILOT_PROMPT.md`](../COPILOT_PROMPT.md)

## Keep It Reviewable

For any task:

1. Keep the change shape small and reviewable.
2. Prefer repo-local templates and examples over free-form generation.
3. Use a documented build, test, or smoke verification path.
4. Record open questions and residual risk instead of hiding them in code.
