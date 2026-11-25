using Claims.Api.Application.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Claimant notifications built by the Kafka consumer from claim events.</summary>
[ApiController]
[Route("api/notifications")]
public class NotificationsController(NotificationService notifications) : ControllerBase
{
    /// <summary>A user's notifications, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NotificationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> List([FromQuery] Guid userId, CancellationToken ct) =>
        Ok(await notifications.ListAsync(userId, ct));

    /// <summary>Mark one of your notifications as read.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType<NotificationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> MarkRead(Guid id, MarkNotificationReadRequest request, CancellationToken ct) =>
        await notifications.MarkReadAsync(id, request, ct);
}
