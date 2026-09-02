namespace SMC.Application.DTOs;

public class DemandWorkflowDto
{
    public int Id { get; set; }
    public int DemandApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public string SpaceRequirement { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string ApplicationStatus { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public decimal PayableAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentLink { get; set; }
    public string? Utr { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentScreenshotFileName { get; set; }
    public string? PaymentScreenshotPath { get; set; }
    public string? RejectionReason { get; set; }
    public string? CertificateFileName { get; set; }
    public string? CertificateFilePath { get; set; }
}

public class ProcessedDemandWorkflowDto
{
    public DemandWorkflowDto Workflow { get; set; } = new();
    public string Action { get; set; } = string.Empty;
    public DateTime ActionAt { get; set; }
}

public class PaymentConfirmationDto
{
    public string Utr { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}

public class RazorpayPaymentDto
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class RazorpayOrderDto
{
    public string KeyId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Currency { get; set; } = "INR";
}

public class PublicPaymentDto
{
    public int DemandApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public decimal PayableAmount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public string? CertificateFileName { get; set; }
    public string? CertificateFilePath { get; set; }
}

public class PublicDemandApplicationStatusDto
{
    public int DemandApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal? PayableAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public bool PaymentAccessGranted { get; set; }
    public bool HasDocumentRequest { get; set; }
    public int? RequestedDocumentId { get; set; }
    public string? RequestedDocumentType { get; set; }
    public string? RequestedDocumentName { get; set; }
    public string? RequestRemark { get; set; }
    public DateTime? RequestDate { get; set; }
    public string? RequestStatus { get; set; }
    public bool CanResubmitRequestedDocument { get; set; }
    public PublicWorkflowLevelDto Je { get; set; } = new();
    public PublicWorkflowLevelDto Os { get; set; } = new();
    public PublicWorkflowLevelDto AssistantCommissioner { get; set; } = new();
}

public class PublicWorkflowLevelDto
{
    public string Status { get; set; } = "Pending";
    public DateTime? ActionAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? PaymentStatus { get; set; }
}
