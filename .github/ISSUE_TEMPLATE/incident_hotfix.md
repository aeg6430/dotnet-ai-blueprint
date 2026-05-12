---
name: Incident hotfix
about: Track urgent production fixes with clear blast-radius, rollback, and verification details
title: "[Hotfix] "
labels: bug, hotfix
assignees: ''
---

## Incident summary

Describe the incident and why it requires urgent handling.

## Severity / impact

- [ ] Sev 1 - production outage
- [ ] Sev 2 - major user-facing degradation
- [ ] Sev 3 - limited impact or degraded internal operations

## Current effect

What is happening right now?

- affected endpoint / page / job / integration:
- affected environment:
- affected users / tenant / role:
- first observed time:

## Immediate symptom

What do users or operators see?

- error response:
- data inconsistency:
- timeout / latency:
- missing side effect:

## Reproduction or trigger

If known, describe the trigger clearly.

1. 
2. 
3. 

If the issue is not safely reproducible, explain the triggering pattern instead.

## Evidence

- incident / alert link:
- correlation / trace ID:
- log excerpt summary:
- DB or downstream-system observation:

## Suspected blast radius

What could this touch if fixed incorrectly?

- [ ] transaction boundary
- [ ] external integration
- [ ] auth / permission path
- [ ] configuration / environment behavior
- [ ] shared legacy hotspot
- [ ] unknown / still being investigated

## Temporary mitigation

Document any immediate workaround already applied.

- feature flag / config toggle:
- manual data correction:
- operator workaround:
- none yet

## Hotfix plan

Describe the smallest safe fix.

- target path:
- code area:
- why this is the minimal safe change:

## Rollback plan

If the hotfix fails, how do we back out safely?

- deployment rollback:
- config rollback:
- data rollback / compensation:

## Verification after deploy

State the fastest reliable checks.

- smoke path:
- log / trace confirmation:
- DB or side-effect confirmation:
- business confirmation:

## Follow-up after stabilization

List what still needs proper cleanup after the incident is contained.

- regression test:
- root-cause cleanup:
- transaction / logging / config debt:
- postmortem / ADR / docs update:
