# Minimal API transactions (optional)

Use this optional module only if your project uses **Minimal APIs** (or you are migrating from MVC controllers).

## Goal

Keep the same transaction strategy as MVC:

- Opt-in via a marker
- Boundary owns `Begin/Commit/Rollback`
- Services throw; boundary rolls back; outer exception handler sends audit/outbox using an **independent connection**

## Suggested shape

- Marker: reuse `[Transactional]` semantics by attaching endpoint metadata, e.g. `.WithMetadata(new TransactionalAttribute())`.
- Boundary: implement an `IEndpointFilter` (example below) that:
  - checks the marker from `HttpContext.GetEndpoint().Metadata`
  - begins a transaction before executing the handler
  - commits on success
  - rolls back and rethrows on failure

## Templates

- `TransactionService.cs.txt`: extracted core boundary logic (reusable in MVC filter / endpoint filter)
- `TransactionEndpointFilter.cs.txt`: Minimal API filter that calls `TransactionService`

