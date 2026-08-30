using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.EntityType).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.FileName).IsRequired().HasMaxLength(300);
        builder.Property(d => d.StoredFileName).IsRequired().HasMaxLength(300);
        builder.Property(d => d.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(d => d.ContentType).HasMaxLength(100);
        builder.Property(d => d.CreatedBy).HasMaxLength(100);
        builder.Property(d => d.UpdatedBy).HasMaxLength(100);
        builder.Property(d => d.DeletedBy).HasMaxLength(100);

        builder.HasOne(d => d.Property).WithMany(p => p.Documents)
            .HasForeignKey(d => d.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Lease).WithMany(l => l.Documents)
            .HasForeignKey(d => d.LeaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.RecoveryCase).WithMany(r => r.Documents)
            .HasForeignKey(d => d.RecoveryCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.SchemeApplication).WithMany(s => s.Documents)
            .HasForeignKey(d => d.SchemeApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.AllocationProcess).WithMany(a => a.Documents)
            .HasForeignKey(d => d.AllocationProcessId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Calculation).WithMany(c => c.Documents)
            .HasForeignKey(d => d.CalculationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.EntityType, d.EntityId });
    }
}
