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

    /// <summary>Also load the synthetic claims data set (ADR-0021). Seeding only runs where MigrateOnStartup is on.</summary>
    public bool SeedSampleData { get; init; }
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

        await RenameLegacyHistoryColumnsAsync(db, cancellationToken);

        logger.LogInformation("Applying database migrations");
        await db.Database.MigrateAsync(cancellationToken);

        foreach (var seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
        {
            logger.LogInformation("Running seeder {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(db, cancellationToken);
        }
    }

    /// <summary>
    /// ADR-0019: the snake_case naming convention also applies to EF's own history table. A database created before it
    /// has the columns <c>MigrationId</c> and <c>ProductVersion</c>, which EF could no longer read. Renames them once.
    /// Does nothing on a new database. Runs only where <c>Database:MigrateOnStartup</c> is on (development and tests).
    /// </summary>
    private static Task RenameLegacyHistoryColumnsAsync(ClaimsDbContext db, CancellationToken cancellationToken) =>
        db.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = '__EFMigrationsHistory' AND column_name = 'MigrationId')
                THEN
                    ALTER TABLE "__EFMigrationsHistory" RENAME COLUMN "MigrationId" TO migration_id;
                    ALTER TABLE "__EFMigrationsHistory" RENAME COLUMN "ProductVersion" TO product_version;
                END IF;
            END $$;
            """,
            cancellationToken);
}
