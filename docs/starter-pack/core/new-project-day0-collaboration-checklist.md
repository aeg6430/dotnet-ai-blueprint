# New Project Day 0 Collaboration Checklist

Use this checklist when starting a new API or API + Razor project and you cannot assume:

- everyone uses Cursor
- everyone uses the same AI tooling
- CI is ready on day 0
- the team already shares the same engineering habits

The goal is to create a project that is still understandable and operable when tooling is inconsistent.

For the broader starter-pack context, see [`../README.md`](../README.md). For the project structure and boundary rules behind this checklist, see [`../../ARCHITECTURE.md`](../../ARCHITECTURE.md), [`../../rules/architecture-protocol.md`](../../rules/architecture-protocol.md), [`../../rules/transactions.md`](../../rules/transactions.md), and [`../../rules/resilience.md`](../../rules/resilience.md).

## Day 0 definition of done

You are ready for collaborative implementation when all of these are true:

- the team has one written source of truth for the first feature scope
- local setup is documented and repeatable
- project structure and ownership boundaries are documented inside the repo
- every engineer can run the same build, test, and smoke commands locally
- the first vertical slice proves the architecture works end to end

Do not wait for perfect automation before establishing these basics.

## 1. Convert raw requirements into engineering language

If the incoming requirement is written in plain business language, convert it into a working spec before coding.

At minimum, the first feature spec should define:

- use cases
- request / response contract
- validation rules
- state transitions
- authorization rules
- expected failure cases
- external dependencies
- non-functional expectations:
  - timeout
  - logging / audit
  - trace / correlation
  - idempotency for write operations

Recommended layout:

- raw notes under `docs/requirements/raw/`
- implementation-ready feature spec under `docs/specs/`

If a rule is unclear, record the gap explicitly. Do not hide requirement ambiguity inside code.

## 2. Put the collaboration contract inside the repo

When tools differ, the repository must carry more of the coordination load.

Create these repo-local entrypoints early:

- `README.md`
  - how to build
  - how to run locally
  - how to execute the main verification commands
- `docs/ARCHITECTURE.md`
  - layering, ownership, main boundaries
- `docs/specs/`
  - the active engineering contract for features
- `CONTRIBUTING.md`
  - branch / review expectations
  - commit / PR expectations
  - minimum verification before merge
- `.editorconfig`
  - baseline formatting and common style defaults
- `scripts/` or equivalent command entrypoints
  - `build`
  - `test`
  - `smoke`

These files matter more than team folklore, chat history, or one AI prompt.

## 3. Decide the technical skeleton on day 0

Use a clear layered shape from the start. See [`../../ARCHITECTURE.md`](../../ARCHITECTURE.md).

### API-only project

Establish:

- API layer for HTTP controllers, filters, models, DI composition, and exception boundary
- Core layer for use cases, services, validation, and interfaces
- Infrastructure layer for repositories, adapters, persistence, and integration details

### API + Razor project

Establish:

- Razor pages or MVC controllers for UI flow and view-model binding only
- Core services for business rules and orchestration
- Infrastructure for DB, external APIs, files, and persistence details

Do not let page models become the place where business rules accumulate.

## 4. Lock in the cross-cutting decisions early

Before the second or third feature, write down these defaults:

- configuration source and environment strategy
- authentication and authorization boundaries
- transaction policy
- logging and correlation policy
- error handling policy
- external integration boundaries

### Configuration

- define where configuration comes from in each environment
- avoid reading environment names directly throughout the codebase
- prefer typed options / centralized config access over scattered lookups

### Transactions

Follow the explicit short-lived UoW model from day 0. See [`../../rules/transactions.md`](../../rules/transactions.md).

- read-only flows do not open transactions
- local write flows begin late and commit early
- remote IO must not happen while the DB transaction is active

### Outbound integrations

Keep outbound HTTP, MQ, SMTP, or file delivery behind interfaces owned by Core and implemented in Infrastructure. Apply timeout / retry / breaker policy in adapters, not in business logic. See [`../../rules/resilience.md`](../../rules/resilience.md).

### Logging and errors

- establish one global exception boundary
- use structured logging
- include correlation identifiers where possible
- never leak raw driver exceptions or secrets to clients

## 5. Prove the architecture with one vertical slice

Do not start by building every shared abstraction. Build one end-to-end slice that proves the chosen structure works:

1. request arrives
2. validation runs
3. use case / service executes
4. repository or adapter is called
5. response is returned
6. logs and failures behave as expected

For API + Razor, this can be one page flow backed by one real use case. For API-only, it can be one create or query endpoint.

That first slice becomes the copy pattern for the rest of the team.

## 6. Standardize local commands before CI exists

If CI does not exist yet, define the local commands as if CI already depends on them.

At minimum, agree on commands for:

- build
- tests
- smoke validation
- formatting or linting, if enabled

Examples:

```text
dotnet build
dotnet test
dotnet run --project src/MyApi
```

The exact commands vary by repo. The important part is that every collaborator uses the same entrypoints.

Later, when CI is introduced, it should call these commands directly rather than inventing a different workflow.

## 7. Set a minimum collaboration rule even without CI

If there is no pipeline yet, use process discipline instead of pretending quality gates already exist.

Require at least:

1. one spec or acceptance note per feature
2. one repeatable local verification step per change
3. one reviewer-readable summary of what changed and how it was checked

This is weaker than CI, but much stronger than "it worked on my machine".

## 8. Keep the first automation goals modest

Do not try to automate everything on day 0. Start with the highest-signal checks:

- build must pass
- smoke path must run
- one or more targeted tests around changed behavior
- formatting baseline via `.editorconfig`

When the project stabilizes, add the stronger guardrails from the starter pack:

- layering tests
- repository / service / API firewall tests
- analyzer severity rollout

See [`../../rules/testing.md`](../../rules/testing.md) and [`../../optional/automation/automation-coverage.md`](../../optional/automation/automation-coverage.md).

## 9. Suggested Day 0 checklist

Use this as a practical starting checklist.

- [ ] `README.md` explains local setup and run commands
- [ ] `docs/specs/` contains the first implementation-ready feature spec
- [ ] `docs/ARCHITECTURE.md` or equivalent explains the intended layer boundaries
- [ ] `.editorconfig` exists
- [ ] local `build`, `test`, and `smoke` commands are documented
- [ ] one vertical slice works end to end
- [ ] global exception handling is present
- [ ] logging / correlation policy is defined
- [ ] transaction rules are documented for read vs write paths
- [ ] outbound integrations are behind adapters / interfaces
- [ ] at least one reviewer can follow the flow without relying on a specific AI tool

## 10. Practical standard

For a new project with inconsistent tooling, the standard is:

- **spec first**
- **repo-local rules before tool-specific rules**
- **one proven vertical slice before broad abstraction**
- **local command discipline before CI**

That gives the team a base that can survive mixed IDEs, mixed AI assistants, and delayed pipeline work without turning into a new legacy problem immediately.
