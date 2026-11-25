using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Api.Infrastructure.Persistence.Configurations;

public sealed class ClaimStatusChangeConfiguration : IEntityTypeConfiguration<ClaimStatusChange>
{
    public void Configure(EntityTypeBuilder<ClaimStatusChange> builder)
    {
        builder.ToTable("claim_status_changes");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();
        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(h => h.Note).HasMaxLength(2000);
        builder.HasOne<User>().WithMany().HasForeignKey(h => h.ChangedBy).OnDelete(DeleteBehavior.Restrict);
    }
}
