namespace Project.Core.DTOs;

// TEMPLATE — normalized outbox row consumed by the post-commit dispatcher/worker.
public sealed record OutboxMessageRecord(
    string MessageId,
    string MessageType,
    string Payload,
    string Status,
    DateTime CreatedAtUtc,
    int AttemptCount,
    DateTime? LastAttemptAtUtc,
    string? LastError);
