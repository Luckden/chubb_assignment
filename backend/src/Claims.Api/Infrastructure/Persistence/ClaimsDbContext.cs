using Claims.Api.Domain;
using Claims.Api.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Infrastructure.Persistence;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Claim> Claims => Set<Claim>();

    public DbSet<ClaimStatusChange> ClaimStatusChanges => Set<ClaimStatusChange>();

    public DbSet<InformationRequest> InformationRequests => Set<InformationRequest>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var claims = MoveDomainEventsToOutbox();
        var saved = base.SaveChanges(acceptAllChangesOnSuccess);
        claims.ForEach(c => c.ClearDomainEvents());
        return saved;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var claims = MoveDomainEventsToOutbox();
        var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        claims.ForEach(c => c.ClearDomainEvents());
        return saved;
    }

    /// <summary>
    /// Adds one outbox row per raised event so it commits with the state change (RFC-0002 section 4). The events are
    /// cleared only after the save succeeds.
    /// </summary>
    private List<Claim> MoveDomainEventsToOutbox()
    {
        var claims = ChangeTracker.Entries<Claim>()
            .Select(e => e.Entity)
            .Where(c => c.DomainEvents.Count > 0)
            .ToList();

        foreach (var @event in claims.SelectMany(c => c.DomainEvents))
        {
            OutboxMessages.Add(OutboxMessage.For(@event));
        }

        return claims;
    }
}
