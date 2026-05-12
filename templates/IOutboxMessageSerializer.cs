namespace Project.Infrastructure.Helpers;

// TEMPLATE — repositories stay string/row typed; JSON serialization lives in a helper.
public interface IOutboxMessageSerializer
{
    OutboxEnvelope Serialize<TMessage>(TMessage message);
}
