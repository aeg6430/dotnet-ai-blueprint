# ADR-0004: AI-assisted audit and evidence policy is a standard post-rollout step

- **Status**: Accepted
- **Date**: 2026-05-12

## Context

This starter pack already defines structured rollout phases for rules, tests, firewalls, and delivery checklists.

After those phases are in place, teams still need a repeatable final verification step that checks:

- whether the current solution still aligns with `docs/ARCHITECTURE.md` and `docs/rules/**`
- whether AI-generated or migrated code left behind starter-pack residue such as placeholder tokens, sample namespaces, or boilerplate comments
- whether audit evidence is stored in a form that can be reviewed later by humans, leads, or auditors

If this step remains a loose chat habit, different teams and AI assistants will produce different report shapes, different evidence bundles, and different residue checks. That makes audit results harder to compare and easier to misread.

The pack therefore needs a stable policy for post-rollout AI-assisted audit behavior, report format, and evidence hierarchy.

## Decision

This pack adopts **Phase E: AI-assisted audit** as a standard post-rollout verification step after Phases A-D.

Phase E is not a replacement for architecture tests, firewalls, analyzers, or delivery checklists. It is a final governance pass that consolidates compliance evidence and highlights residue or gray areas that automated gates may not fully describe.

The standard Phase E outputs are:

- `compliance-audit-report.md`
- architecture-test pass logs and/or screenshots
- relevant `artifacts/` outputs referenced by the audit
- optional `chat-transcript.pdf` as **supplementary** evidence only

The Phase E report must include a **Markdown-table compliance matrix** mapped to the relevant `docs/rules/*.md` and related architecture guidance.

The Phase E audit must also perform fixed residue and hygiene checks, including:

- namespace residue such as `Skeleton`, `Acme`, `Project.*`, `starter-pack`, `seed`, or `skeleton`
- placeholder tokens such as `{Solution}`, `{CoreNamespace}`, `{DB_PASSWORD}`, and similar setup markers
- sample or development connection strings that should not survive in a target project
- boilerplate comments such as `TODO: Replace this in your project`

The evidence hierarchy for Phase E is:

- **Primary**: `compliance-audit-report.md`
- **Primary**: architecture-test pass logs/screenshots and relevant `artifacts/` outputs
- **Supplementary**: exported chat transcript PDF

Because AI can produce false positives or incomplete interpretations, the Phase E report requires **human-in-the-loop review/sign-off** before it is treated as an accepted audit snapshot.

## Consequences

- **Positive**:
  - Gives humans and AI assistants a repeatable final audit step after rollout.
  - Produces a more stable, comparable governance artifact than ad hoc chat summaries.
  - Makes residue checks explicit instead of relying on memory or one-off searches.
  - Clarifies that chat transcripts support the audit trail but do not replace primary technical evidence.
- **Negative / trade-offs**:
  - Adds another delivery step and review artifact to maintain.
  - AI-assisted audit can still produce false positives or miss context-heavy edge cases.
  - Teams must keep the report prompt and evidence expectations aligned with evolving rules.
- **Follow-ups**:
  - Keep the root `README.md` Phase E workflow aligned with this ADR.
  - Keep `docs/starter-pack/README.md` aligned with the portable adoption path for Phase E.
  - When rule coverage or evidence expectations change materially, update this ADR or supersede it with a newer audit-policy ADR.

## Links

- Spec:
  - `README.md`
  - `docs/ARCHITECTURE.md`
  - `docs/rules/architecture-protocol.md`
  - `docs/rules/transactions.md`
  - `docs/rules/resilience.md`
  - `docs/optional/automation/automation-coverage.md`
- Code:
  - `docs/starter-pack/architecture-tests/GenericLayeringArchitectureTests.cs.txt`
  - `docs/starter-pack/architecture-tests/GenericApiFirewallArchitectureTests.cs.txt`
  - `docs/starter-pack/architecture-tests/GenericServiceFirewallArchitectureTests.cs.txt`
  - `docs/starter-pack/architecture-tests/GenericRepositoryFirewallArchitectureTests.cs.txt`
  - `docs/starter-pack/architecture-tests/PlaceholderGuardTests.cs.txt`
  - `docs/starter-pack/architecture-tests/ExceptionLeakTests.cs.txt`
- Related ADRs:
  - `docs/adr/0001-explicit-short-lived-uow.md`
  - `docs/adr/0002-polly-style-outbound-resilience.md`
  - `docs/adr/0003-minimal-api-transaction-wrapper-limited-use.md`
