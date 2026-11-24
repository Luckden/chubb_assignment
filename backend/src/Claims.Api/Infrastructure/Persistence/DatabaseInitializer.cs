using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Infrastructure.Persistence;

/// <summary>Seeds data after migrations run. Implementations must be idempotent: they run on every startup.</summary>
public interface IDataSeeder
{
    Task SeedAsync(ClaimsDbContext db, CancellationToken cancellationToken);
}

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply EF Core migrations and run seeders on startup. Development/test only.</summary>
    public bool MigrateOnStartup { get; init; }
}

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var options = app.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        if (!options.MigrateOnStartup)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ClaimsDbContext>>();

        logger.LogInformation("Applying database migrations");
        await db.Database.MigrateAsync(cancellationToken);

        foreach (var seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
        {
            logger.LogInformation("Running seeder {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(db, cancellationToken);
        }
    }
}
