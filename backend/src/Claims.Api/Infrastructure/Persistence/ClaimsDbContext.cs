using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Infrastructure.Persistence;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Claim> Claims => Set<Claim>();

    public DbSet<ClaimStatusChange> ClaimStatusChanges => Set<ClaimStatusChange>();

    public DbSet<InformationRequest> InformationRequests => Set<InformationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }
}
