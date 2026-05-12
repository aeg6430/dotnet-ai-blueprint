---
name: Bug report
about: Report a bug with reproduction, impact, and verification details
title: "[Bug] "
labels: bug
assignees: ''
---

## Summary

Describe the bug in 1-3 sentences.

## Impact

What is affected?

- [ ] Production outage
- [ ] User-facing error
- [ ] Wrong data
- [ ] Performance degradation
- [ ] Logging / observability gap
- [ ] Internal tooling / docs issue

## Where it happens

- endpoint / page / job / batch:
- environment:
- account / role / tenant, if relevant:

## Reproduction

Provide a repeatable path. Be specific.

1. 
2. 
3. 

### Sample input

Include request payload, query, page action, or record identifiers if safe to share.

```text
```

## Expected behaviour

What should happen instead?

## Actual behaviour

What happened instead?

## Evidence

Attach or describe the strongest evidence you have.

- response / screenshot:
- log / trace / correlation ID:
- DB observation:
- incident / ticket link:

## Suspected area

If you have a guess, point to the likely path.

- controller / page / job:
- service / use case:
- repository / query / integration:

## Transaction / side-effect risk

Mark any that seem relevant.

- [ ] multiple writes may be inconsistent
- [ ] remote IO may overlap with DB transaction
- [ ] retry / duplicate side effects may be involved
- [ ] config or environment branching may be involved
- [ ] auth / permission behavior may be involved

## Verification after fix

How should we verify the fix?

- automated test:
- smoke / manual path:
- expected DB or side-effect observation:

## Additional context

Anything else reviewers or implementers should know:

- regression history:
- workaround:
- residual risk if fixed narrowly:
