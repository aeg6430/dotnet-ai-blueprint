namespace Project.Infrastructure.Helpers;

// TEMPLATE — normalized outbox payload returned by the serializer helper.
public sealed record OutboxEnvelope(string MessageId, string MessageType, string Payload);
