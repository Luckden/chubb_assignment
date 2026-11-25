namespace Claims.Api.Domain;

/// <summary>
/// A message for a claimant, built by the notifications consumer from a lifecycle event (RFC-0002 section 5).
/// <see cref="EventId"/> is unique, so a redelivered event cannot create a second notification.
/// </summary>
public sealed class Notification
{
    private Notification()
    {
    }

    public Notification(Guid userId, Guid claimId, Guid eventId, string message, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        ClaimId = claimId;
        EventId = eventId;
        Message = message;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ClaimId { get; private set; }

    /// <summary>The event that caused this notification; the consumer's idempotency key.</summary>
    public Guid EventId { get; private set; }

    public string Message { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>Marking an already read notification keeps the first read time.</summary>
    public void MarkRead(DateTimeOffset now) => ReadAt ??= now;
}
