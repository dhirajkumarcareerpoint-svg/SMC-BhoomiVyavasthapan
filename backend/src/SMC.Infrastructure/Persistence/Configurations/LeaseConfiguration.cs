using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> builder)
    {
        builder.ToTable("Leases");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LesseeName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.LesseeMobile).HasMaxLength(15);
        builder.Property(l => l.LesseeAddress).HasMaxLength(500);
        builder.Property(l => l.DeedNumber).IsRequired().HasMaxLength(100);
        builder.Property(l => l.DurationType).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.RentAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.SecurityDeposit).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Shera).HasMaxLength(2000);
        builder.Property(l => l.CreatedBy).HasMaxLength(100);
        builder.Property(l => l.UpdatedBy).HasMaxLength(100);
        builder.Property(l => l.DeletedBy).HasMaxLength(100);

        builder.HasOne(l => l.Property).WithMany(p => p.Leases)
            .HasForeignKey(l => l.PropertyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.DeedNumber);
        builder.HasIndex(l => l.Status);
    }
}
