namespace Claims.Api.Infrastructure.Messaging;

/// <summary>Publishes integration events to the message bus.</summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(string topic, string key, TEvent @event, CancellationToken cancellationToken = default);
}
