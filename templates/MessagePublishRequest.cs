namespace Project.Core.DTOs;

// TEMPLATE — outbox-backed broker publish request with explicit routing and payload shape.
public sealed record MessagePublishRequest(
    string RequestId,
    string MessageType,
    string PartitionKey,
    string PayloadJson);
