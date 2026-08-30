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

public class PaymentConfirmationDto
{
    public string Utr { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
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
