using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Infrastructure.Persistence.Seeders;

/// <summary>Fixed demo users so the API can be tried without authentication (acting users are passed in requests).</summary>
public static class DemoUsers
{
    public static readonly Guid Alice = Guid.Parse("00000000-0000-7000-8000-000000000001");
    public static readonly Guid Bob = Guid.Parse("00000000-0000-7000-8000-000000000002");
    public static readonly Guid Carol = Guid.Parse("00000000-0000-7000-8000-000000000011");
    public static readonly Guid Dave = Guid.Parse("00000000-0000-7000-8000-000000000012");

    public const string Team = "team-a";

    public static IReadOnlyList<User> All { get; } =
    [
        new(Alice, "Alice Tan (claimant)", UserRole.Claimant),
        new(Bob, "Bob Lim (claimant)", UserRole.Claimant),
        new(Carol, "Carol Wong (officer)", UserRole.Officer, Team),
        new(Dave, "Dave Ng (officer)", UserRole.Officer, Team),
    ];
}

public sealed class DemoUserSeeder : IDataSeeder
{
    public async Task SeedAsync(ClaimsDbContext db, CancellationToken cancellationToken)
    {
        var existing = await db.Users.Select(u => u.Id).ToListAsync(cancellationToken);
        var missing = DemoUsers.All.Where(u => !existing.Contains(u.Id)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        db.Users.AddRange(missing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
