using System;
using System.Text.Json;

namespace Project.Infrastructure.Helpers;

// TEMPLATE — centralize message serialization outside repositories to satisfy the repository firewall.
public sealed class OutboxMessageSerializer : IOutboxMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxEnvelope Serialize<TMessage>(TMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var runtimeType = message.GetType();
        var payload = JsonSerializer.Serialize(message, runtimeType, SerializerOptions);
        var messageType = runtimeType.Name;
        var requestId = runtimeType.GetProperty("RequestId")?.GetValue(message) as string;
        var messageId = string.IsNullOrWhiteSpace(requestId)
            ? Guid.NewGuid().ToString("N")
            : $"{messageType}:{requestId}";

        return new OutboxEnvelope(messageId, messageType, payload);
    }
}
