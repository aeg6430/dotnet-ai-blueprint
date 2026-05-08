---
inclusion: manual
---

# Security: SAST (rules)

This document provides a **copyable baseline** for integrating Static Application Security Testing (SAST) into delivery workflows.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: start in **report-only** mode (observation), focus on new/changed code first, and remediate high severity findings with a defined backlog.
- **Strict (new project)**: enforce SAST findings as a **release check threshold** from day 0, with explicit exception handling and review.

## Objectives

- Provide a repeatable, auditable SAST execution path in CI.
- Produce traceable artifacts for delivery (report + exceptions + remediation tracking).
- Align findings with OWASP ASVS mapping where applicable.

## Minimum CI behavior (recommended)

- Run SAST in CI on:
  - pull requests (report-only or blocking depending on adoption profile)
  - main branch builds (always generate an artifact)
- Store artifacts under:
  - `artifacts/security/` (or your CI artifact store)

## Minimum deliverables (evidence)

- SAST report (per build / per release)
- Exception list (if any), including:
  - finding id or signature
  - rationale
  - scope (paths)
  - expiry date / review date
- Remediation tracking (issue/backlog reference)

## ASVS alignment

- When a finding maps to an ASVS requirement, record the mapping in the ASVS checklist/template.
- When a finding is accepted as an exception, record the rationale and review cadence.

## Air-gapped environments

If the build environment is air-gapped:

- Prefer a SAST tool that supports offline execution, or provide an internal mirror for signatures/rules.
- Treat tool updates as a controlled change with recorded versioning.

