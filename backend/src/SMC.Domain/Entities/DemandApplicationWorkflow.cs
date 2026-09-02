using SMC.Domain.Common;

namespace SMC.Domain.Entities;

public class DemandApplicationWorkflow : BaseEntity
{
    public int DemandApplicationId { get; set; }
    public DemandApplication DemandApplication { get; set; } = null!;
    public string Stage { get; set; } = "JEVerificationPending";
    public string? RejectionReason { get; set; }
    public decimal PayableAmount { get; set; }
    public string PaymentStatus { get; set; } = "NotRequired";
    public string? PaymentLink { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }
    // A high-entropy capability token proves possession of the payment link.  It is
    // deliberately separate from the application number so a number alone cannot
    // be used to view or submit a payment.
    public string? PaymentAccessToken { get; set; }
    public string? Utr { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentScreenshotPath { get; set; }
    public string? PaymentScreenshotFileName { get; set; }
    public long? PaymentScreenshotSizeBytes { get; set; }
    public string? PaymentSubmittedBy { get; set; }
    public DateTime? PaymentSubmittedAt { get; set; }
    public string? PaymentVerifiedBy { get; set; }
    public DateTime? PaymentVerifiedAt { get; set; }
    public string? CertificateFilePath { get; set; }
    public string? CertificateFileName { get; set; }
    public DateTime? CertificateGeneratedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
