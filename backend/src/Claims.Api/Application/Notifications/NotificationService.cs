using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Application.Notifications;

/// <summary>Reads and marks claimant notifications (RFC-0002 section 5).</summary>
public sealed class NotificationService(
    ClaimsDbContext db,
    TimeProvider clock,
    IValidator<MarkNotificationReadRequest> markReadValidator)
{
    /// <summary>A user's notifications, newest first.</summary>
    public async Task<IReadOnlyList<NotificationResponse>> ListAsync(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("userId", "'User Id' must not be empty.")]);
        }

        return await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
            .Take(100)
            .Select(n => new NotificationResponse(n.Id, n.ClaimId, n.Message, n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);
    }

    public async Task<NotificationResponse> MarkReadAsync(Guid id, MarkNotificationReadRequest request, CancellationToken ct)
    {
        await markReadValidator.ValidateAndThrowAsync(request, ct);

        var notification = await db.Notifications.SingleOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new NotFoundException($"Notification {id} was not found.");
        if (notification.UserId != request.UserId)
        {
            throw new NotPermittedException("Only the recipient can mark this notification as read.");
        }

        notification.MarkRead(clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return new NotificationResponse(notification.Id, notification.ClaimId, notification.Message, notification.CreatedAt, notification.ReadAt);
    }
}
