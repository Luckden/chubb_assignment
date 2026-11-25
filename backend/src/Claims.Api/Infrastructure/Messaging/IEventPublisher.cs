namespace Claims.Api.Infrastructure.Messaging;

/// <summary>Publishes events to the message bus.</summary>
public interface IEventPublisher
{
    /// <summary>Serialises <paramref name="event"/> as JSON and publishes it.</summary>
    Task PublishAsync<TEvent>(string topic, string key, TEvent @event, CancellationToken cancellationToken = default);

    /// <summary>Publishes an already serialised payload. Used by the outbox relay and the dead-letter path.</summary>
    Task PublishRawAsync(
        string topic,
        string key,
        string payload,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
