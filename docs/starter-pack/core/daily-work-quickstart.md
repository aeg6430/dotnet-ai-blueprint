# Daily Work Quickstart

Use this one-page checklist when you need a fast starting point for common day-to-day work.

This page is intentionally short and uses neutral task language. For the full rule set, follow the linked documents.

## General Defaults

Use these defaults for every task unless a spec or target repository says otherwise:

1. Read the relevant spec first, if one exists.
2. Keep the change shape small and reviewable.
3. Verify the changed path before and after the edit.
4. Reuse repo-local templates and examples before creating a new pattern.
5. Record ambiguity, residual risk, or follow-up work explicitly.

Start from these shared references:

- [`../../START_HERE.md`](../../START_HERE.md)
- [`../../../CONTRIBUTING.md`](../../../CONTRIBUTING.md)
- [`../../ARCHITECTURE.md`](../../ARCHITECTURE.md)
- [`../../rules/architecture-protocol.md`](../../rules/architecture-protocol.md)

## 1. Small Feature

Use this path when you are adding or extending behavior in an existing flow without changing the overall project structure.

### Read First

- [`../../specs/`](../../specs/)
- [`legacy-bugfix-feature-sop.md`](legacy-bugfix-feature-sop.md)
- [`../../../templates/`](../../../templates/)

### Start Checklist

1. Write down the exact input, output, and acceptance rule for the change.
2. Identify the minimum path you need to touch: entry point, service/use case, data access, mapping, config.
3. Check whether the change touches read-only flow, local write flow, or cross-system flow.
4. Reuse the nearest existing pattern before adding a new structure.
5. Define one repeatable verification method before editing.

### First Pass Complete When

- the change path is clear
- the required files and layers are identified
- one verification path is written down

## 2. Defect Fix

Use this path when existing behavior is wrong, unstable, inconsistent, or producing the wrong data.

### Read First

- [`legacy-bugfix-feature-sop.md`](legacy-bugfix-feature-sop.md)
- [`../../../CONTRIBUTING.md`](../../../CONTRIBUTING.md)
- the relevant spec under [`../../specs/`](../../specs/), if one exists

### Start Checklist

1. Capture one clear reproduction with exact inputs or steps.
2. Write the expected result and the current incorrect result.
3. Map only the changed path: entry point, main service chain, data access, external dependency, logs, and user-visible output.
4. Choose the smallest safe correction instead of broad cleanup.
5. Define how you will prove the fix worked and how you will spot rollback conditions.

### First Pass Complete When

- the problem is reproducible
- the suspected path is narrowed down
- the verification method is explicit

## 3. New Project Handoff

Use this path when you are taking over a new or early-stage project, or when the project structure is still forming.

### Read First

- [`new-project-day0-collaboration-checklist.md`](new-project-day0-collaboration-checklist.md)
- [`../README.md`](../README.md)
- [`../../specs/feature-spec-template.md`](../../specs/feature-spec-template.md)

### Start Checklist

1. Identify the current source of truth for scope, commands, and boundaries.
2. Confirm whether the first feature is defined in an implementation-ready spec.
3. Check whether local build, test, and run/smoke commands are documented.
4. Confirm where API, core logic, persistence, and external dependency responsibilities belong.
5. Pick one small vertical slice that proves the current structure works end to end.

### First Pass Complete When

- the current scope and boundaries are visible in the repo
- the local commands are known
- one small end-to-end slice is identified

## 4. External Integration

Use this path when the task touches outbound HTTP, messaging, SMTP, file delivery, shared services, or third-party systems.

### Read First

- [`../../rules/external-integration-firewall.md`](../../rules/external-integration-firewall.md)
- [`../../rules/anti-corruption-layer.md`](../../rules/anti-corruption-layer.md)
- [`../../rules/resilience.md`](../../rules/resilience.md)
- [`../../rules/transactions.md`](../../rules/transactions.md)

### Start Checklist

1. Identify the external dependency, its contract, and its failure modes.
2. Confirm which part of the flow is local and which part crosses a system boundary.
3. Check whether any active database transaction overlaps the outbound work.
4. Define timeout, retry, and error-handling expectations before editing.
5. Keep translation between local models and external models at the boundary.

### First Pass Complete When

- the system boundary is explicit
- transaction and outbound timing are clear
- failure handling and verification are defined

## Done For The Day Means

Before you stop or hand off, leave behind:

- the task type you followed
- the path you changed
- the verification method
- any remaining risk, open question, or follow-up item

If the task spans more than one category, combine the relevant sections above and follow the stricter rule where they differ.
