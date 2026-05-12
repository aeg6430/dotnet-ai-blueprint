## Summary

Describe the change in 1-3 bullets.

- 
- 

## Why

Explain the reason for this change.

- bugfix
- feature delivery
- risk reduction
- refactor needed to support adjacent work

## Spec / Source

Link the engineering spec or source material that drove this change.

- Spec: 
- Raw requirement / ticket / incident: 

If no spec exists, explain why:

- 

## Change Type

Mark the primary shape of this change.

- [ ] New / greenfield feature work
- [ ] Legacy bugfix
- [ ] Legacy feature work
- [ ] Risk-reduction / maintenance
- [ ] Docs / template / process only

## Verification

List exactly how you verified the change.

- Build:
- Tests:
- Smoke / manual verification:

### Expected evidence

- response / UI result:
- DB or side-effect observation:
- logs / audit / traces checked:

If automation is not available, say what was verified manually.

## Risk / Impact

Call out the main review risks.

- transaction boundary changed? 
- external dependency touched? 
- config or environment behavior changed? 
- auth / permission behavior changed? 
- legacy hotspot touched? 

## Residual Risk / Follow-up

List anything intentionally not addressed in this PR.

- 
- 

## Reviewer Notes

Anything the reviewer should inspect closely:

- 
