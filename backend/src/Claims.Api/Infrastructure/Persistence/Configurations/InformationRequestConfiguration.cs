using Claims.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Api.Infrastructure.Persistence.Configurations;

public sealed class InformationRequestConfiguration : IEntityTypeConfiguration<InformationRequest>
{
    public void Configure(EntityTypeBuilder<InformationRequest> builder)
    {
        builder.ToTable("information_requests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Question).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.Response).HasMaxLength(2000);
        builder.Ignore(r => r.IsOpen);
        builder.HasOne<User>().WithMany().HasForeignKey(r => r.RequestedBy).OnDelete(DeleteBehavior.Restrict);
    }
}
