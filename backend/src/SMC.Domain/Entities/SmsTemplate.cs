using SMC.Domain.Common;

namespace SMC.Domain.Entities;

/// <summary>Office-submitted, provider-neutral SMS template. MessageBody is never rewritten at runtime.</summary>
public class SmsTemplate : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public string VariableMapping { get; set; } = "[]";
    public string? DltTemplateId { get; set; }
    public string? SenderId { get; set; }
    public string Language { get; set; } = "mr";
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string ApprovalStatus { get; set; } = "PendingApproval";
    public string? SampleMessage { get; set; }
}
