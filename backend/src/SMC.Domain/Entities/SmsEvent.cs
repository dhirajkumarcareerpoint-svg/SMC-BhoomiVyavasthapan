using SMC.Domain.Common;

namespace SMC.Domain.Entities;

/// <summary>Provider-neutral, durable record of an SMS business event.</summary>
public class SmsEvent : BaseEntity
{
    public string RecipientMobile { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? ApplicationNumber { get; set; }
    public string? TemplateReference { get; set; }
    public string Status { get; set; } = "Queued";
    public string? ProviderMessageId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}
