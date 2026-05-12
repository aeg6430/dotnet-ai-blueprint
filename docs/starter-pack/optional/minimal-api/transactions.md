# Minimal API transactions (optional)

Use this optional module only if your project uses **Minimal APIs** (or you are migrating from MVC controllers).

## Goal

Provide a **narrow convenience wrapper** for Minimal API endpoints that are:

- local-write only
- short-lived
- free of outbound network IO

This is **not** the starter-pack default transaction strategy. The default is the explicit use-case UoW described in [`../../core/transactions.md`](../../core/transactions.md) and the binding rules in [`../../../rules/transactions.md`](../../../rules/transactions.md).

## Suggested shape

- Marker: attach endpoint metadata, e.g. `.WithMetadata(new TransactionalAttribute())`.
- Boundary: implement an `IEndpointFilter` (example below) that:
  - checks the marker from `HttpContext.GetEndpoint().Metadata`
  - begins a transaction before executing the local write handler
  - commits on success
  - rolls back and rethrows on failure

## Do not use this module when

- the handler calls an external API
- the handler does long-running CPU work
- the handler publishes directly to MQ/webhook/email inside the request path
- the handler should instead use local write + outbox

For those cases, keep the transaction explicit inside the use-case/service so it starts late and ends early.

## Templates

- `TransactionService.cs.txt`: extracted boundary helper for short local write endpoints only
- `TransactionEndpointFilter.cs.txt`: Minimal API filter that calls `TransactionService`

