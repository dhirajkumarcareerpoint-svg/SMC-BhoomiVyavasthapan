using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class CalculationConfiguration : IEntityTypeConfiguration<Calculation>
{
    public void Configure(EntityTypeBuilder<Calculation> builder)
    {
        builder.ToTable("Calculations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Rate).HasColumnType("decimal(18,2)");
        builder.Property(c => c.PreviousOutstanding).HasColumnType("decimal(18,2)");
        builder.Property(c => c.CurrentDemand).HasColumnType("decimal(18,2)");
        builder.Property(c => c.CalculatedAmount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Shera).HasMaxLength(2000);
        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);
        builder.Property(c => c.DeletedBy).HasMaxLength(100);

        builder.HasOne(c => c.Property).WithMany(p => p.Calculations)
            .HasForeignKey(c => c.PropertyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.PropertyId);
        builder.HasIndex(c => c.Status);
    }
}
