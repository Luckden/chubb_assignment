using FluentValidation;

namespace Claims.Api.Application.Notifications;

// No authentication yet (ADR-0009): the request names the acting user, as the claims endpoints do.

/// <summary>A claimant marks one of their notifications as read.</summary>
public sealed record MarkNotificationReadRequest(Guid UserId);

public sealed record NotificationResponse(
    Guid Id,
    Guid ClaimId,
    string Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);

public sealed class MarkNotificationReadRequestValidator : AbstractValidator<MarkNotificationReadRequest>
{
    public MarkNotificationReadRequestValidator() => RuleFor(r => r.UserId).NotEmpty();
}
