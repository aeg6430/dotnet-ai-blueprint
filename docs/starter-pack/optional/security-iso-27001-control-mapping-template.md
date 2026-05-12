# Security / compliance template: ISO 27001 control mapping

This file is a **copy/paste template** to help a project map blueprint rules and implementation evidence to an ISO 27001-oriented control review.

Use it together with:

- `docs/rules/audit-log.md`
- `docs/rules/security-sast.md`
- `docs/adr/0004-ai-assisted-audit-and-evidence-policy.md`
- `docs/starter-pack/optional/security-compliance-audit-report-template.md`

## How to use

- Pick the control set used by your organization (for example Annex A / internal control IDs).
- Keep requirement summaries short and evidence concrete.
- Mark uncertain mappings as **manual review required** instead of forcing a pass/fail judgment.

## Project context

- **Project / repo**:
- **System type**: internal / external-facing / mixed / batch-heavy
- **Data classification**:
- **Deployment model**: on-prem / cloud / hybrid
- **Control catalog version**:
- **Reviewer**:

## Control mapping

| Control ID | Control Objective | Blueprint Rule / ADR / Evidence Source | Implementation Evidence | Status | Follow-up |
|---|---|---|---|---|---|
| A.x.x | Example: logging and monitoring | `docs/rules/audit-log.md` | boundary middleware, SIEM route, sample audit log | Pass | None |
| A.x.x | Example: secure development lifecycle | `docs/rules/architecture-protocol.md`, architecture tests | CI logs, test paths, PR checklist | Pass | None |
| A.x.x | Example: vulnerability management | `docs/rules/security-sast.md` | SAST report, exception approval record | Review required | Confirm exception owner |

## Supporting evidence checklist

- Audit / traceability evidence:
- Secure development / architecture evidence:
- Access control evidence:
- Vulnerability management evidence:
- Incident / hotfix evidence:
- Backup / recovery / operational evidence:

## Control gaps or exceptions

- Control ID:
  - Gap or exception:
  - Compensating control:
  - Approval / owner:
  - Target date:

## Manual review required

- Mapping item:
  - Reason:
  - Reviewer needed:
  - Exit criterion:

## Notes

- This template does **not** replace your organization's formal ISMS documentation.
- Use it as a project-level mapping aid that ties code, rules, tests, and evidence back to the expected controls.
