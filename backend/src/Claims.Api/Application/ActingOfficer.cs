using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Application;

/// <summary>
/// The team and exposure views are for officers only (RFC-0003, ADR-0008). Until authentication exists the caller
/// names themselves in the query string (ADR-0009).
/// </summary>
internal static class ActingOfficer
{
    /// <summary>Returns the officer, or throws 400 for a missing id and 403 for anyone who is not an officer.</summary>
    public static async Task<User> RequireAsync(ClaimsDbContext db, Guid officerId, CancellationToken ct)
    {
        if (officerId == Guid.Empty)
        {
            throw new ValidationException([new ValidationFailure("officerId", "'Officer Id' must not be empty.")]);
        }

        return await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == officerId && u.Role == UserRole.Officer, ct)
            ?? throw new NotPermittedException($"User {officerId} is not a known Officer.");
    }
}
