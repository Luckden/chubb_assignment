using Claims.Api.Infrastructure.Persistence;
using Claims.Api.Infrastructure.Persistence.Seeders;
using Claims.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Api.Tests;

/// <summary>Integration tests against real PostgreSQL (Testcontainers). Requires Docker.</summary>
[Collection(ApiCollection.Name)]
public class SeedingTests(ClaimsApiFactory factory)
{
    private static readonly Guid[] DemoUserIds = [.. DemoUsers.All.Select(u => u.Id)];

    [Fact]
    public async Task Startup_Seeding_CreatesTheDemoUsers()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        var count = await db.Users.CountAsync(u => DemoUserIds.Contains(u.Id));

        Assert.Equal(DemoUserIds.Length, count);
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotDuplicateUsers()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var seeder = new DemoUserSeeder();
        await seeder.SeedAsync(db, CancellationToken.None);

        await seeder.SeedAsync(db, CancellationToken.None);

        Assert.Equal(DemoUserIds.Length, await db.Users.CountAsync(u => DemoUserIds.Contains(u.Id)));
    }
}
