# Contributing Guide

This repository is a portable .NET starter pack. Treat it as a source of truth for rules, templates, and collaboration patterns rather than as a finished product application.

This guide is intentionally **tool-neutral**. Follow it whether you use Cursor, Visual Studio, VS Code, GitHub Copilot, another assistant, or direct editing.

## Start here

Before making substantial changes, read:

1. [`README.md`](README.md)
2. [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
3. [`docs/rules/architecture-protocol.md`](docs/rules/architecture-protocol.md)
4. [`docs/rules/transactions.md`](docs/rules/transactions.md)
5. The relevant spec under [`docs/specs/`](docs/specs/), if one exists

If your task is setup-related, read [`docs/starter-pack/project-setup-protocol.md`](docs/starter-pack/project-setup-protocol.md) before editing.

## Working model

Use these defaults unless the target repo or spec says otherwise:

- **Spec first**: raw requirement notes belong in `docs/requirements/raw/`; implementation-ready behavior belongs in `docs/specs/`
- **Layering first**: keep API, Core, and Infrastructure responsibilities explicit
- **Transaction first**: no remote IO inside active DB transactions
- **Minimal change shape**: prefer small reviewable changes over broad cleanup
- **Repo-local truth**: important rules should live in files, not only in chat history

## Contribution types

### New / greenfield work

- write or update the relevant feature spec first
- establish the smallest vertical slice that proves the architecture
- prefer repo-local commands and templates over tool-specific magic

### Legacy / existing project work

- follow the legacy-safe path from [`docs/starter-pack/core/legacy-bugfix-feature-sop.md`](docs/starter-pack/core/legacy-bugfix-feature-sop.md)
- prioritize reproducibility, bounded blast radius, and verification over cleanup

## Before you implement

For feature work:

- confirm the feature spec exists or create/update it
- identify transaction boundaries and external dependencies
- note any ambiguity or blind spots explicitly

For bugfix work:

- capture a clear reproduction
- define the expected result
- choose the smallest safe change path
- decide how the fix will be verified

## Verification

If CI is available, use it.

If CI is not available yet, every change should still have a repeatable verification path:

- documented local build command
- documented test or smoke command
- verification checklist when automation is not yet practical

Do not rely on "it worked once on my machine" as the only evidence.

If your repository uses a PR workflow, prefer the repo-local template at [`.github/pull_request_template.md`](.github/pull_request_template.md) so verification and residual risk are stated explicitly.
If your repository uses GitHub issues, prefer the repo-local templates under [`.github/ISSUE_TEMPLATE/`](.github/ISSUE_TEMPLATE/) so bug reports and feature requests start with enough context to become engineering work.
For urgent production fixes, prefer [`.github/ISSUE_TEMPLATE/incident_hotfix.md`](.github/ISSUE_TEMPLATE/incident_hotfix.md) so rollback and blast-radius thinking are captured up front.

## Suggested local commands

Projects built from this starter pack should standardize local entrypoints such as:

```text
dotnet build
dotnet test
dotnet run --project <your-api-project>
```

Document the real commands in the target repo's `README.md`.

In this repository itself, the documented native commands remain valid. When repeatable setup or verification commands are needed, prefer the root `Makefile` as the stable command surface:

```text
make init TARGET_PROJECT_NAME=Acme.Ordering
make setup-scan SETUP_ROOT=.
make setup-rewrite-placeholders TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
make setup-rewrite-content TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
make setup-rename-solution-projects TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
make setup-rename-paths TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
make setup-clean SETUP_ROOT=.
make setup-verify SETUP_ROOT=.
make build   # dotnet build skeleton/StarterPack.Skeleton.sln -c Release
make test    # dotnet test skeleton/StarterPack.Skeleton.sln -c Release
make test-edge
make commit-msg-check MSG="docs(ai): clarify setup flow"
```

## About the Makefile

The `Makefile` in this repo is the preferred bounded automation surface. The docs and underlying commands remain valid without it.

It may contain reviewed setup or verification logic that replaces the old `.ps1` entrypoints, provided the behavior stays explicit, parameterized, and limited to starter-pack setup concerns.

Allowed examples include placeholder replacement, content-level identity alignment, solution/project renames, explicit path renames, or controlled cleanup tied to setup flows. Prefer splitting these into small reviewed setup targets rather than hiding them in one opaque command. Do not place business logic, product-specific behavior, hidden target-repo assumptions, or opaque side effects into the `Makefile`.

The workflow must remain understandable from the repo docs so contributors can still execute it directly when `make` is unavailable or not desired.

Use the `Makefile` for repeatable command surfaces such as build/test entrypoints, focused guardrail checks, and the mechanical part of Conventional Commits validation. Keep TDD, edge-first prioritization, review scope, and similar engineering judgment in the docs and review process rather than trying to encode all of that thinking into a target.

The bundled skeleton targets `net8.0` so the same validation path can run under either the .NET 8 SDK or the .NET 9 SDK.
The repo's GitHub Actions workflow mirrors that same path and validates the skeleton on both SDK lines.

If you are working Windows-native without GNU Make, run the underlying `dotnet` commands directly and follow `docs/starter-pack/project-setup-protocol.md` for setup work.

## Pull request / review expectations

Keep changes reviewable. A good change set should tell the reviewer:

- what changed
- why it changed
- how it was verified
- what was intentionally left out
- any residual risk or follow-up

If the task touches a legacy hotspot, say so directly.

## Do not add new debt casually

Avoid introducing these into changed paths:

- hardcoded environment detection in business logic
- interpolated SQL
- remote IO inside active transactions
- sync-over-async (`.Result`, `.Wait()`)
- direct leakage of internal exception details to clients

## When rules and reality conflict

If the target project or feature spec intentionally differs from the default starter-pack assumptions:

- follow the spec first
- document the decision
- prefer an ADR or explicit note over silent divergence

## Useful repo entrypoints

- Starter-pack overview: [`docs/starter-pack/README.md`](docs/starter-pack/README.md)
- Daily work quickstart: [`docs/starter-pack/core/daily-work-quickstart.md`](docs/starter-pack/core/daily-work-quickstart.md)
- Legacy maintenance SOP: [`docs/starter-pack/core/legacy-bugfix-feature-sop.md`](docs/starter-pack/core/legacy-bugfix-feature-sop.md)
- New-project Day 0 checklist: [`docs/starter-pack/core/new-project-day0-collaboration-checklist.md`](docs/starter-pack/core/new-project-day0-collaboration-checklist.md)
- Feature spec template: [`docs/specs/feature-spec-template.md`](docs/specs/feature-spec-template.md)
- Filled feature spec example: [`docs/specs/example-warehouse-create.md`](docs/specs/example-warehouse-create.md)
- PR template: [`.github/pull_request_template.md`](.github/pull_request_template.md)
- Bug issue template: [`.github/ISSUE_TEMPLATE/bug_report.md`](.github/ISSUE_TEMPLATE/bug_report.md)
- Feature issue template: [`.github/ISSUE_TEMPLATE/feature_request.md`](.github/ISSUE_TEMPLATE/feature_request.md)
- Incident hotfix issue template: [`.github/ISSUE_TEMPLATE/incident_hotfix.md`](.github/ISSUE_TEMPLATE/incident_hotfix.md)
