# Security / compliance audit report template

This file is a **copy/paste template** for the `compliance-audit-report.md` artifact required by Phase E.

Use it after feature completion or before PR submission to produce a concise, reviewable evidence bundle that aligns with:

- `docs/ARCHITECTURE.md`
- `docs/rules/*.md`
- `.cursor/rules/*.mdc`
- `docs/adr/0004-ai-assisted-audit-and-evidence-policy.md`

## How to use

- Keep the report short outside the compliance matrix.
- Prefer concrete file/test/config evidence over chat-only summaries.
- If a finding is uncertain, mark it as **manual review required** instead of declaring full compliance.

## Report header

- **Project / repo**:
- **Scope reviewed**:
- **Date**:
- **Reviewer**:
- **Branch / commit**:
- **Audit trigger**: pre-PR / pre-release / post-rollout / periodic review

## Executive summary

- **Overall status**: Pass / Pass with follow-up / Review required / Fail
- **High-risk findings**:
- **Primary evidence attached**:
- **Supplementary evidence attached**:

## Compliance Matrix

| Rule Source | Requirement Summary | Evidence | Status | Follow-up |
|---|---|---|---|---|
| `docs/rules/architecture-protocol.md` | Example: Core does not depend on Infrastructure outside allowed boundaries | `tests/Architecture/LayeringArchitectureTests.cs`, CI log, PR link | Pass | None |
| `docs/rules/transactions.md` | Example: no remote IO while the main DB transaction is active | service code path, unit test, architecture review note | Review required | Validate one legacy write path manually |
| `docs/rules/audit-log.md` | Example: API boundary captures actor/action/target/result/correlation | middleware/filter code, sample log, test | Pass | None |

Add one row per relevant rule, ADR-backed exception, or project-specific requirement in scope.

## Confirmed evidence

- Architecture test logs / screenshots:
- Relevant `artifacts/` outputs:
- PR / commit links:
- Config / environment evidence:
- Security scan / SAST evidence:

## Suspected violations

- Item:
  - Evidence:
  - Risk:
  - Recommended action:

## Manual review required

- Item:
  - Why automation is insufficient:
  - Reviewer needed:
  - Exit criterion:

## Residue and hygiene checks

Record whether any of the following remain in the reviewed scope:

- **Namespace residue**: `Skeleton`, `Acme`, `Project.*`, `starter-pack`, `seed`, `skeleton`
- **Placeholder residue**: `{Solution}`, `{CoreNamespace}`, `{InfrastructureNamespace}`, `{ApiNamespace}`, `{TestsNamespace}`, `{DB_PASSWORD}`
- **Connection string / secret residue**: sample credentials, default passwords, placeholder secrets
- **Boilerplate comments**: `TODO: Replace this in your project` and similar starter-pack leftovers

| Check | Result | Evidence | Follow-up |
|---|---|---|---|
| Namespace residue | Pass / Fail / Review required | search result / file path |  |
| Placeholder residue | Pass / Fail / Review required | search result / file path |  |
| Secret residue | Pass / Fail / Review required | search result / file path |  |
| Boilerplate comments | Pass / Fail / Review required | search result / file path |  |

## Evidence inventory

| Evidence Type | Location | Level | Notes |
|---|---|---|---|
| `compliance-audit-report.md` | repo root or release artifacts | Primary | Required |
| Architecture test logs / screenshots | `artifacts/`, CI, or attached evidence | Primary | Required when available |
| Additional generated artifacts | `artifacts/` | Primary | Optional, scope-dependent |
| Chat transcript PDF | attachment / export | Supplementary | Never the only evidence |

## Sign-off

- **Human reviewer**:
- **Decision**: Accept / Accept with follow-up / Rework required
- **Notes**:
