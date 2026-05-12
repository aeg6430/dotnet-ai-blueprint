namespace Project.Infrastructure.Adapters;

// TEMPLATE — options bound from configuration for a broker-style publisher adapter.
public sealed class MessagePublisherOptions
{
    public const string SectionName = "MessagePublisher";

    public string BrokerName { get; init; } = "ExampleBroker";
    public string TopicName { get; init; } = "domain-events";
    public int PublishTimeoutSeconds { get; init; } = 8;
}
