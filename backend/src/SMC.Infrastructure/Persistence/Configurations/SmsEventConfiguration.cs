using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class SmsEventConfiguration : IEntityTypeConfiguration<SmsEvent>
{
    public void Configure(EntityTypeBuilder<SmsEvent> builder)
    {
        builder.ToTable("SmsEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecipientMobile).IsRequired().HasMaxLength(15);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(80);
        builder.Property(x => x.ApplicationNumber).HasMaxLength(40);
        builder.Property(x => x.TemplateReference).HasMaxLength(200);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(30);
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ApplicationNumber, x.EventType }).IsUnique();
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
