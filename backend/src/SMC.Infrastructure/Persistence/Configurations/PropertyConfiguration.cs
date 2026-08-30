using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(40);
        builder.Property(p => p.PropertyCode).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(250);
        builder.Property(p => p.Ward).HasMaxLength(50);
        builder.Property(p => p.Zone).HasMaxLength(50);
        builder.Property(p => p.Address).HasMaxLength(500);
        builder.Property(p => p.AreaSqFt).HasColumnType("decimal(18,2)");
        builder.Property(p => p.MonthlyRent).HasColumnType("decimal(18,2)");
        builder.Property(p => p.AnnualDemand).HasColumnType("decimal(18,2)");
        builder.Property(p => p.SurveyNumber).HasMaxLength(50);
        builder.Property(p => p.TpNumber).HasMaxLength(50);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.CurrentOccupant).HasMaxLength(200);
        builder.Property(p => p.Shera).HasMaxLength(2000);
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);
        builder.Property(p => p.DeletedBy).HasMaxLength(100);

        builder.HasIndex(p => p.PropertyCode);
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Ward);
    }
}
