using Claims.Api.Domain;
using Claims.Api.Infrastructure.Messaging;
using Claims.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Application.Notifications;

/// <summary>
/// Turns "needs more information" and "decision made" events into claimant notifications (RFC-0002 section 5).
/// The unique event id on the notification makes redelivery harmless.
/// </summary>
public sealed class ClaimantNotificationHandler(ClaimsDbContext db, TimeProvider clock) : IEventHandler
{
    public bool CanHandle(string eventType) =>
        eventType is nameof(Domain.Events.InformationRequested)
            or nameof(Domain.Events.ClaimSettled)
            or nameof(Domain.Events.ClaimRejected);

    public async Task HandleAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (await db.Notifications.AnyAsync(n => n.EventId == envelope.EventId, cancellationToken))
        {
            return;
        }

        var claim = await db.Claims
            .AsNoTracking()
            .Where(c => c.Id == envelope.ClaimId)
            .Select(c => new { c.ClaimantId, c.ClaimNumber })
            .SingleOrDefaultAsync(cancellationToken);
        if (claim is null)
        {
            return;
        }

        var message = envelope.EventType == nameof(Domain.Events.InformationRequested)
            ? $"Your claim {claim.ClaimNumber} needs more information."
            : $"A decision has been made on your claim {claim.ClaimNumber}.";

        db.Notifications.Add(new Notification(claim.ClaimantId, envelope.ClaimId, envelope.EventId, message, clock.GetUtcNow()));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (!await db.Notifications.AnyAsync(n => n.EventId == envelope.EventId, cancellationToken))
            {
                throw;
            }

            // A concurrent copy of the same event won the unique index.
        }
    }
}
