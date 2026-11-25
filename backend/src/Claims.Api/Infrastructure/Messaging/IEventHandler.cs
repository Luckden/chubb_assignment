namespace Claims.Api.Infrastructure.Messaging;

/// <summary>
/// Reacts to events from <see cref="Topics.ClaimEvents"/>. Delivery is at least once, so a handler must tolerate seeing
/// the same <see cref="EventEnvelope.EventId"/> twice and finish its work in one transaction (RFC-0002 section 8).
/// Handlers are resolved in a new scope per message.
/// </summary>
public interface IEventHandler
{
    bool CanHandle(string eventType);

    Task HandleAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
