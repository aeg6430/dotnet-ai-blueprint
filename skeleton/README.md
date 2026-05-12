# Skeleton Runnable Reference

This runnable reference now maps the starter-pack outbound patterns into a minimal self-contained app.

## Included mappings

- `payment` via `Acme.Core.Outbound.PaymentAuthorizeRequest` + `Acme.Infrastructure.Outbound.PaymentGateway`
- `webhook` via `Acme.Core.Outbound.WebhookDeliveryRequest` + `Acme.Infrastructure.Outbound.WebhookGateway`
- `messaging` via `Acme.Core.Outbound.MessagePublishRequest` + `Acme.Infrastructure.Outbound.MessagePublisher`
- `post-commit delivery` via `Acme.Infrastructure.Outbound.InMemoryOutboxRepository`, `OutboxDispatcher`, and `OutboxDeliveryWorker`

## Demo endpoints

- `POST /demo/outbox/payment`
- `POST /demo/outbox/webhook`
- `POST /demo/outbox/message`
- `GET /demo/outbox/pending`

The API keeps the runnable reference offline-friendly by simulating outbound delivery while preserving the same queue-then-dispatch shape used by the starter-pack templates.
