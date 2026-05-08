# ADR (engineering habits)

This pack includes an ADR template to keep important engineering decisions stable over time.

## Why ADRs exist (what changes)

- **For humans**: reduce repeated debates and keep the “why” behind a decision available to reviewers and newcomers.
- **For AI assistants**: give a stable set of decisions to reference so the assistant does not invent new conventions or dependencies.

## When to write an ADR (common triggers)

- Introducing a new dependency/framework (or changing the approved list).
- Changing **transaction ownership** (service-owned vs boundary-owned).
- Changing **exception boundary** behavior (what can/cannot leak to HTTP).
- Adding/removing/loosening architecture gates (layering/firewalls/analyzers).
- Adopting audit/outbox strategy, idempotency strategy, or SLA/error-code mapping.

## What an ADR should contain

- **Context**: current problem and constraints.
- **Decision**: what we will do.
- **Alternatives**: what we considered and why not chosen.
- **Consequences**: trade-offs, risks, and operational impact.
- **Rollout**: legacy-safe migration plan and how to validate.

## What ADRs should avoid

- Business/domain specifics that don’t generalize.
- Meeting transcripts or “everyone feels”.
- Unverifiable statements without consequences/rollout.

