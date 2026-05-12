namespace Project.Infrastructure.Adapters;

// TEMPLATE — worker settings for post-commit outbox delivery.
public sealed class OutboxDeliveryOptions
{
    public const string SectionName = "OutboxDelivery";

    public int BatchSize { get; init; } = 20;
    public int PollIntervalSeconds { get; init; } = 10;
}
