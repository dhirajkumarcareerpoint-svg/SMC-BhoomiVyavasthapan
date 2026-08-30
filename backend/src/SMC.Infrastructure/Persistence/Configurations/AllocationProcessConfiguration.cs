using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class AllocationProcessConfiguration : IEntityTypeConfiguration<AllocationProcess>
{
    public void Configure(EntityTypeBuilder<AllocationProcess> builder)
    {
        builder.ToTable("AllocationProcesses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Method).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.NoticeNumber).HasMaxLength(100);
        builder.Property(a => a.ReserveAmount).HasColumnType("decimal(18,2)");
        builder.Property(a => a.HighestBidAmount).HasColumnType("decimal(18,2)");
        builder.Property(a => a.HighestBidderName).HasMaxLength(200);
        builder.Property(a => a.HighestBidderMobile).HasMaxLength(15);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Shera).HasMaxLength(2000);
        builder.Property(a => a.CreatedBy).HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);
        builder.Property(a => a.DeletedBy).HasMaxLength(100);

        builder.HasOne(a => a.Property).WithMany(p => p.AllocationProcesses)
            .HasForeignKey(a => a.PropertyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.Method);
        builder.HasIndex(a => a.Status);
    }
}
