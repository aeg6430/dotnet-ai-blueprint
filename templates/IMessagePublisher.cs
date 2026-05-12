using Project.Core.DTOs;

namespace Project.Core.Interfaces;

// TEMPLATE — Core port for post-commit broker publication.
public interface IMessagePublisher
{
    Task PublishAsync(
        MessagePublishRequest request,
        CancellationToken cancellationToken);
}
