using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Infrastructure.Persistence.Seeders;

/// <summary>
/// Loads the synthetic APAC claims data (ADR-0021) when <c>Database:SeedSampleData</c> is on: about 1,400 claims with
/// their history and information requests. Every sample claimant is <see cref="DemoUsers.SampleClaimant"/> and every
/// sample officer is <see cref="DemoUsers.SampleStaff"/>, so a rejected claim still names who handled it. Rows already
/// present are skipped, and an earlier load that used one user per source person is folded onto these two. It writes
/// no outbox events, because these facts predate the system.
/// </summary>
public sealed class SampleDataSeeder(IConfiguration configuration, ILogger<SampleDataSeeder> logger) : IDataSeeder
{
    private const int BatchSize = 200;

    public async Task SeedAsync(ClaimsDbContext db, CancellationToken cancellationToken)
    {
        var options = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        if (!options.SeedSampleData)
        {
            return;
        }

        var data = SampleDataLoader.Load();

        var existingUsers = (await db.Users.Select(u => u.Id).ToListAsync(cancellationToken)).ToHashSet();
        db.Users.AddRange(data.Users.Where(u => !existingUsers.Contains(u.Id)));
        await db.SaveChangesAsync(cancellationToken);

        var existingClaims = (await db.Claims.Select(c => c.Id).ToListAsync(cancellationToken)).ToHashSet();
        var missing = data.Claims.Where(c => !existingClaims.Contains(c.Id)).ToList();
        foreach (var batch in missing.Chunk(BatchSize))
        {
            db.Claims.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }

        var folded = await FoldLegacySampleUsersAsync(db, cancellationToken);
        logger.LogInformation(
            "Sample data: {Claims} claims added, {Skipped} already present, {Folded} earlier sample users folded onto SampleClaimant and Sample Staff",
            missing.Count,
            data.Claims.Count - missing.Count,
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
