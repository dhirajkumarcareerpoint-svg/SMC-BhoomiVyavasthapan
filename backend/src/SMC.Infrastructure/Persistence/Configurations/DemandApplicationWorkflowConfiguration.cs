using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class DemandApplicationWorkflowConfiguration : IEntityTypeConfiguration<DemandApplicationWorkflow>
{
    public void Configure(EntityTypeBuilder<DemandApplicationWorkflow> builder)
    {
        builder.ToTable("DemandApplicationWorkflows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Stage).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PaymentStatus).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PaymentAccessToken).HasMaxLength(128);
        builder.Property(x => x.PayableAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.DemandApplicationId).IsUnique();
        builder.HasIndex(x => new { x.Stage, x.PaymentStatus });
        builder.HasOne(x => x.DemandApplication).WithOne().HasForeignKey<DemandApplicationWorkflow>(x => x.DemandApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}
