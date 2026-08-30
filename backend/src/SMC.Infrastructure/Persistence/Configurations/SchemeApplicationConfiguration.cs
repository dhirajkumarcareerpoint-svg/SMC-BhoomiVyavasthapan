using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class SchemeApplicationConfiguration : IEntityTypeConfiguration<SchemeApplication>
{
    public void Configure(EntityTypeBuilder<SchemeApplication> builder)
    {
        builder.ToTable("SchemeApplications");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SchemeType).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.ApplicantName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ApplicantMobile).HasMaxLength(15);
        builder.Property(s => s.OriginalOutstanding).HasColumnType("decimal(18,2)");
        builder.Property(s => s.WaivedAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.PayableAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ApprovedBy).HasMaxLength(150);
        builder.Property(s => s.Shera).HasMaxLength(2000);
        builder.Property(s => s.CreatedBy).HasMaxLength(100);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);
        builder.Property(s => s.DeletedBy).HasMaxLength(100);

        builder.HasOne(s => s.Property).WithMany(p => p.SchemeApplications)
            .HasForeignKey(s => s.PropertyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.SchemeType);
    }
}
