using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Api.Infrastructure.Persistence.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claims");
        builder.HasKey(c => c.Id);

        // Keys are generated in the domain (Guid v7). Without this, EF treats a new child with a key as existing.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ClaimNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(c => c.ClaimNumber).IsUnique();

        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.Description).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.ClaimedAmount).HasPrecision(18, 2);
        builder.Property(c => c.DecisionOutcome).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.DecisionReason).HasMaxLength(2000);

        // Optimistic concurrency on PostgreSQL xmin: a second officer picking up the same claim gets a conflict.
        builder.Property(c => c.Version).IsRowVersion();

        builder.HasOne<User>().WithMany().HasForeignKey(c => c.ClaimantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(c => c.AssigneeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.StatusHistory).WithOne().HasForeignKey(h => h.ClaimId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.InformationRequests).WithOne().HasForeignKey(r => r.ClaimId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.InformationRequests).UsePropertyAccessMode(PropertyAccessMode.Field);

        // The officer queue: unassigned and assigned claims by status, oldest first.
        builder.HasIndex(c => new { c.Status, c.SubmittedAt });
        builder.HasIndex(c => c.AssigneeId);
    }
}
