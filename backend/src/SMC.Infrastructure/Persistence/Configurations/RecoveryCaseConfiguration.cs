using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class RecoveryCaseConfiguration : IEntityTypeConfiguration<RecoveryCase>
{
    public void Configure(EntityTypeBuilder<RecoveryCase> builder)
    {
        builder.ToTable("RecoveryCases");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.OutstandingAmount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.RecoveredAmount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Stage).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.NoticeNumber).HasMaxLength(100);
        builder.Property(r => r.Shera).HasMaxLength(2000);
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.UpdatedBy).HasMaxLength(100);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        builder.HasOne(r => r.Property).WithMany(p => p.RecoveryCases)
            .HasForeignKey(r => r.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Lease).WithMany(l => l.RecoveryCases)
            .HasForeignKey(r => r.LeaseId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Stage);
    }
}
