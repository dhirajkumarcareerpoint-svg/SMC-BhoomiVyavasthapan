using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Persistence.Configurations;

public class SmsTemplateConfiguration : IEntityTypeConfiguration<SmsTemplate>
{
    public void Configure(EntityTypeBuilder<SmsTemplate> builder)
    {
        builder.ToTable("SmsTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(80);
        builder.Property(x => x.TemplateName).IsRequired().HasMaxLength(160);
        builder.Property(x => x.MessageBody).IsRequired();
        builder.Property(x => x.VariableMapping).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.DltTemplateId).HasMaxLength(200);
        builder.Property(x => x.SenderId).HasMaxLength(30);
        builder.Property(x => x.Language).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ApprovalStatus).IsRequired().HasMaxLength(40);
        builder.HasIndex(x => new { x.EventType, x.IsActive });
    }
}
