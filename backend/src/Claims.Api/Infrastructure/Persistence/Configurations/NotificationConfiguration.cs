using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Api.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.Message).HasMaxLength(500).IsRequired();

        // Consumer idempotency: a second copy of the same event hits this index and is skipped.
        builder.HasIndex(n => n.EventId).IsUnique();
        builder.HasIndex(n => new { n.UserId, n.CreatedAt });

        builder.HasOne<User>().WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Claim>().WithMany().HasForeignKey(n => n.ClaimId).OnDelete(DeleteBehavior.Cascade);
    }
}
