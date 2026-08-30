using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class DemandApplicationConfiguration : IEntityTypeConfiguration<DemandApplication>
{
    public void Configure(EntityTypeBuilder<DemandApplication> builder)
    {
        builder.ToTable("DemandApplications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApplicationNumber).IsRequired().HasMaxLength(40);
        builder.Property(x => x.ApplicantAccessTokenHash).HasMaxLength(64);
        builder.HasIndex(x => x.ApplicationNumber).IsUnique();
        builder.Property(x => x.ServiceType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.BusinessType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.ApplicantName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Mobile).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.GstNumber).IsRequired().HasMaxLength(15);
        builder.Property(x => x.PermanentAddress).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.CorrespondenceAddress).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.State).IsRequired().HasMaxLength(100);
        builder.Property(x => x.District).IsRequired().HasMaxLength(100);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Taluka).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PinCode).IsRequired().HasMaxLength(6);
        builder.Property(x => x.Zone).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Prabhag).IsRequired().HasMaxLength(30);
        builder.Property(x => x.AreaSqFt).HasColumnType("decimal(18,2)");
        builder.Property(x => x.FeeAmount).HasColumnType("decimal(18,2)");
        builder.HasMany(x => x.Documents).WithOne(x => x.DemandApplication).HasForeignKey(x => x.DemandApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}

public class DemandApplicationDocumentConfiguration : IEntityTypeConfiguration<DemandApplicationDocument>
{
    public void Configure(EntityTypeBuilder<DemandApplicationDocument> builder)
    {
        builder.ToTable("DemandApplicationDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.HasIndex(x => x.DemandApplicationId);
    }
}
