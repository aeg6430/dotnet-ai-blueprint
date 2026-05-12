---
name: Feature request
about: Describe a feature request in a way that can be turned into an engineering spec
title: "[Feature] "
labels: enhancement
assignees: ''
---

## Summary

Describe the requested feature in plain business language.

## Problem / Goal

What problem does this solve, or what outcome do we want?

## Who needs this

- actor / role:
- internal or external user:
- business owner / stakeholder:

## Main use case

Describe the primary happy path in simple steps.

1. 
2. 
3. 

## Expected result

What should the user or downstream system be able to do after this is delivered?

## Source material

Link or describe the raw source material that should feed the spec.

- ticket / issue:
- meeting notes:
- document / spreadsheet / screenshot:
- related incident or workaround:

## Proposed interface

If known, describe the likely entry point.

- [ ] API endpoint
- [ ] Razor page / MVC screen
- [ ] Background job / batch
- [ ] Internal admin / back-office flow
- [ ] External integration
- [ ] Not sure yet

Details:

- route / page / job name, if known:

## Data and rules

List the important business rules or constraints.

- required fields:
- validation rules:
- status / state transitions:
- permission rules:
- tenant / ownership rules:

## External dependencies

Does this depend on another system, vendor API, SMTP, MQ, file store, or scheduled job?

- [ ] No
- [ ] Yes

If yes, describe it:

- dependency:
- purpose:
- what happens if it fails:

## Acceptance / Verification

How will we know this feature is correct?

- sample input:
- expected output or user-visible result:
- expected DB or side-effect observation:
- manual verification path:

## Open questions

List anything that is still unclear.

- 
- 

## Out of scope

List what should **not** be included in this request.

- 
- 

## Next step

Before implementation, convert this request into a spec under `docs/specs/`, ideally starting from `docs/specs/feature-spec-template.md`.
