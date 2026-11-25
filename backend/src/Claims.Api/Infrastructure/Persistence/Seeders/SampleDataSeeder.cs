using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Infrastructure.Persistence.Seeders;

/// <summary>
/// Loads a short sample of the synthetic claims (ADR-0021) when <c>Database:SeedSampleData</c> is on: two claims in
/// each status, all filed by <see cref="DemoUsers.SampleClaimant"/> and handled by <see cref="DemoUsers.SampleStaff"/>.
/// The four demo users stay as themselves. A database that already has the full file is trimmed back to this sample.
/// It writes no outbox events, because these facts predate the system.
/// </summary>
public sealed class SampleDataSeeder(IConfiguration configuration, ILogger<SampleDataSeeder> logger) : IDataSeeder
{
    /// <summary>How many claims of each status a fresh database receives.</summary>
    public const int PerStatus = 2;

    public static IReadOnlyList<Claim> ClaimsToSeed(IReadOnlyList<Claim> claims) =>
        Enum.GetValues<ClaimStatus>().SelectMany(status =>
        {
            var ofStatus = claims.Where(c => c.Status == status);
            var preferred = status == ClaimStatus.Submitted
                ? ofStatus.Where(c => c.AssigneeId is null)
                : ofStatus.Where(c => c.AssigneeId is not null);
            var picked = preferred.Take(PerStatus).ToList();
            return picked.Count == PerStatus ? picked : ofStatus.Take(PerStatus);
        }).ToList();

    public async Task SeedAsync(ClaimsDbContext db, CancellationToken cancellationToken)
    {
        var options = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        if (!options.SeedSampleData)
        {
            return;
        }

        var data = SampleDataLoader.Load();
        var sample = ClaimsToSeed(data.Claims);
        var sampleIds = sample.Select(c => c.Id).ToHashSet();

        var existingUsers = (await db.Users.Select(u => u.Id).ToListAsync(cancellationToken)).ToHashSet();
        db.Users.AddRange(data.Users.Where(u => !existingUsers.Contains(u.Id)));
        await db.SaveChangesAsync(cancellationToken);

        var removed = await db.Claims
            .Where(c => c.ClaimantId == DemoUsers.SampleClaimant && !sampleIds.Contains(c.Id))
            .ExecuteDeleteAsync(cancellationToken);

        var existingClaims = (await db.Claims.Select(c => c.Id).ToListAsync(cancellationToken)).ToHashSet();
        var missing = sample.Where(c => !existingClaims.Contains(c.Id)).ToList();
        if (missing.Count > 0)
        {
            db.Claims.AddRange(missing);
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }

        var folded = await FoldLegacySampleUsersAsync(db, cancellationToken);
        logger.LogInformation(
            "Sample data: {Claims} claims kept ({Added} added, {Removed} removed), {Folded} earlier sample users folded onto SampleClaimant and Sample Staff",
            sample.Count,
            missing.Count,
            removed,
            folded);
    }

    /// <summary>
    /// The first load created one user per source customer and officer. Point their claims, history and questions at
    /// SampleClaimant and Sample Staff, then remove the unused users.
    /// </summary>
    private static async Task<int> FoldLegacySampleUsersAsync(ClaimsDbContext db, CancellationToken cancellationToken)
    {
        var keep = DemoUsers.All.Select(u => u.Id).Append(DemoUsers.SampleClaimant).ToHashSet();
        var legacyClaimants = await db.Users
            .Where(u => u.Role == UserRole.Claimant && !keep.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
        var legacyOfficers = await db.Users
            .Where(u => u.Role == UserRole.Officer && !keep.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
        if (legacyClaimants.Count == 0 && legacyOfficers.Count == 0)
        {
            return 0;
        }

        if (legacyClaimants.Count > 0)
        {
            await db.Claims.Where(c => legacyClaimants.Contains(c.ClaimantId))
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ClaimantId, DemoUsers.SampleClaimant), cancellationToken);
            await db.ClaimStatusChanges.Where(h => legacyClaimants.Contains(h.ChangedBy))
                .ExecuteUpdateAsync(s => s.SetProperty(h => h.ChangedBy, DemoUsers.SampleClaimant), cancellationToken);
            await db.Notifications.Where(n => legacyClaimants.Contains(n.UserId))
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.UserId, DemoUsers.SampleClaimant), cancellationToken);
        }

        if (legacyOfficers.Count > 0)
        {
            await db.Claims.Where(c => c.AssigneeId != null && legacyOfficers.Contains(c.AssigneeId.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.AssigneeId, DemoUsers.SampleStaff), cancellationToken);
            await db.ClaimStatusChanges.Where(h => legacyOfficers.Contains(h.ChangedBy))
                .ExecuteUpdateAsync(s => s.SetProperty(h => h.ChangedBy, DemoUsers.SampleStaff), cancellationToken);
            await db.InformationRequests.Where(r => legacyOfficers.Contains(r.RequestedBy))
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.RequestedBy, DemoUsers.SampleStaff), cancellationToken);
            await db.Notifications.Where(n => legacyOfficers.Contains(n.UserId))
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.UserId, DemoUsers.SampleStaff), cancellationToken);
        }

        var legacy = legacyClaimants.Concat(legacyOfficers).ToArray();
        await db.Users.Where(u => legacy.Contains(u.Id)).ExecuteDeleteAsync(cancellationToken);
        return legacy.Length;
    }
}
