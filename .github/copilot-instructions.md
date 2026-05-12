# Copilot instructions

This file is the Copilot-specific entrypoint. Use it with [`docs/START_HERE.md`](../docs/START_HERE.md), which remains the shared task-routing index.

## Read order (required)

1. [`docs/START_HERE.md`](../docs/START_HERE.md)
2. [`docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md)
3. Rules (binding):
   - [`docs/rules/architecture-protocol.md`](../docs/rules/architecture-protocol.md)
   - [`docs/rules/transactions.md`](../docs/rules/transactions.md)
   - [`docs/rules/resilience.md`](../docs/rules/resilience.md)
   - [`docs/rules/audit-log.md`](../docs/rules/audit-log.md)
   - [`docs/rules/external-integration-firewall.md`](../docs/rules/external-integration-firewall.md)
   - [`docs/rules/anti-corruption-layer.md`](../docs/rules/anti-corruption-layer.md)
   - [`docs/rules/sql.md`](../docs/rules/sql.md)
   - [`docs/rules/mapping.md`](../docs/rules/mapping.md)
   - [`docs/rules/code-quality.md`](../docs/rules/code-quality.md)
   - [`docs/rules/testing.md`](../docs/rules/testing.md)
4. Shadow examples (copy patterns):
   - [`docs/starter-pack/shadow-examples/`](../docs/starter-pack/shadow-examples/)
   - [`templates/`](../templates/)

Also read these when they apply:

- endpoint protection, CI slowness, restricted cloud tooling, or exception-leak prevention: [`docs/rules/endpoint-protection.md`](../docs/rules/endpoint-protection.md)
- incident hotfixes, temporary path blocking, or request screening: [`docs/rules/request-screening.md`](../docs/rules/request-screening.md)
- project setup, namespace conversion, or folder rename: [`docs/starter-pack/project-setup-protocol.md`](../docs/starter-pack/project-setup-protocol.md)

When setup is being automated, prefer the reviewed `Makefile` setup targets over ad hoc rename or cleanup commands. Use the split flow: `setup-scan`, `setup-rewrite-placeholders`, `setup-rewrite-content`, `setup-rename-solution-projects`, `setup-rename-paths`, `setup-clean`, and `setup-verify`.

## Working defaults

1. If a target repo provides `docs/specs/`, follow the relevant spec first.
2. Use `docs/requirements/raw/` as source material, not as the final coding contract.
3. If the spec is incomplete, fall back to this repository's rules and templates.
4. Keep target naming, style, and boundaries instead of copying blueprint-specific names verbatim.
5. For legacy edits, do not introduce `TransactionScope` into paths that already use `IDbTransaction`, and re-check async/await consistency.

## Plan-first preference

For multi-file work or anything that touches layering, transactions, security, or setup, prefer a plan-first flow before applying edits.

If Plan / Agent features are unavailable, start from [`docs/START_HERE.md`](../docs/START_HERE.md), use normal Copilot Chat plus manual edits, and keep the work in small reviewable batches.

## Non-negotiables

- **Layering**: `{CoreNamespace}` must not depend on `{InfrastructureNamespace}` or `{ApiNamespace}` (replace tokens per [`docs/starter-pack/README.md`](../docs/starter-pack/README.md)).
- **Repositories**: SQL + Dapper only. No business rules, no JSON parsing, no `SELECT *`, no interpolated SQL.
- **Services / use cases**: Follow the explicit short-lived UoW rules in [`docs/rules/transactions.md`](../docs/rules/transactions.md): no transaction for read-only flows, no remote IO while the DB transaction is active, begin late, commit early, and prefer outbox for cross-system side effects.
- **API**: Controllers are thin; Infrastructure wiring belongs in DI composition.
- **Audit logging**: Capture security-relevant audit events at the API entry point / global exception boundary per [`docs/rules/audit-log.md`](../docs/rules/audit-log.md); include actor identity, action, target, result, and correlation metadata.
- **Logging**: Prefer `ILogger<T>`; do not leak secrets/PII.
- **Default application boundary**: Prefer native ASP.NET Core mechanisms plus explicit services / use cases; do not introduce MediatR as a starter-pack default without a local ADR.
- **Optional boundary controls**: Keep request-screening style features behind feature-local extension methods so the main DI/composition root keeps a single opt-in line.

## Output requirements

When generating code:

- For **multi-file** work or anything that touches **layering / transactions / security**, prefer a plan-first flow.
- For project setup requests on a Seed folder, use [`docs/starter-pack/project-setup-protocol.md`](../docs/starter-pack/project-setup-protocol.md) before feature implementation or refactoring.
- For automated setup work, call the reviewed `Makefile` targets instead of constructing one-off shell commands for rename, cleanup, or verification.
- If a target repo provides `docs/specs/`, follow the feature spec before applying default blueprint assumptions.
- If the task touches API-edge monitoring, traceability, security review, or audit evidence, also follow [`docs/rules/audit-log.md`](../docs/rules/audit-log.md).
- If the task touches incident hotfixes, temporary path blocking, or malicious parameter filtering, also follow [`docs/rules/request-screening.md`](../docs/rules/request-screening.md).
- If the task touches endpoint protection, CI slowness, Build Agent exclusions, restricted cloud tooling, Apex One, Phase C firewall hardening, or exception-leak prevention, also follow [`docs/rules/endpoint-protection.md`](../docs/rules/endpoint-protection.md) and prioritize `ExceptionLeakTests` before broader firewall expansion.
- Prefer patterns from `docs/starter-pack/shadow-examples/` or `templates/`.
- Keep changes minimal and consistent with analyzers and architecture tests.
- If a rule is unclear, add an ADR or update the rule instead of inventing a new convention.

