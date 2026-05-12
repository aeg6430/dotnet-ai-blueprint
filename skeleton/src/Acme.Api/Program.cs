using Acme.Api.Extensions;
using Acme.Core.Outbound;
using Acme.Core.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOutboundDemo(builder.Configuration);

var app = builder.Build();

// Minimal endpoint to keep the skeleton quiet; cross-cutting boundaries should live at the HTTP edge.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/demo/outbox/payment", async (
    PaymentAuthorizeRequest request,
    OutboundDispatchDemoService service,
    CancellationToken cancellationToken) =>
{
    await service.QueuePaymentAsync(request, cancellationToken);
    return Results.Accepted("/demo/outbox/pending", new { queued = request.RequestId, type = "payment" });
});

app.MapPost("/demo/outbox/webhook", async (
    WebhookDeliveryRequest request,
    OutboundDispatchDemoService service,
    CancellationToken cancellationToken) =>
{
    await service.QueueWebhookAsync(request, cancellationToken);
    return Results.Accepted("/demo/outbox/pending", new { queued = request.RequestId, type = "webhook" });
});

app.MapPost("/demo/outbox/message", async (
    MessagePublishRequest request,
    OutboundDispatchDemoService service,
    CancellationToken cancellationToken) =>
{
    await service.QueueMessageAsync(request, cancellationToken);
    return Results.Accepted("/demo/outbox/pending", new { queued = request.RequestId, type = "message" });
});

app.MapGet("/demo/outbox/pending", async (IOutboxRepository outboxRepository, CancellationToken cancellationToken) =>
{
    var pending = await outboxRepository.GetPendingBatchAsync(50, cancellationToken);
    return Results.Ok(pending);
});

app.Run();
