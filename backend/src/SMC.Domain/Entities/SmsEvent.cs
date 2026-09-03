using SMC.Domain.Common;

namespace SMC.Domain.Entities;

public class SmsEvent : BaseEntity
{
    public string RecipientMobile { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? ApplicationNumber { get; set; }
    public string? TemplateReference { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ProviderMessageId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}
