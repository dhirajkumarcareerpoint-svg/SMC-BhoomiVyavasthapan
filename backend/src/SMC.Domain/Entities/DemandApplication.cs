using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

public class DemandApplication : BaseEntity
{
    public string ApplicationNumber { get; set; } = string.Empty;
    // A hashed, per-application capability used only by the anonymous applicant
    // lifecycle. The raw token is returned once to the browser and is never stored.
    public string? ApplicantAccessTokenHash { get; set; }
    public DemandServiceType ServiceType { get; set; }
    public DemandBusinessType? BusinessType { get; set; }
    public string? OtherBusinessType { get; set; }
    public string? ApplicantType { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? IdentityNumber { get; set; }
    public string? PanNumber { get; set; }
    public string GstNumber { get; set; } = string.Empty;
    public string PermanentAddress { get; set; } = string.Empty;
    public string CorrespondenceAddress { get; set; } = string.Empty;
    public bool SameAddress { get; set; }
    public string State { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Taluka { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Prabhag { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AvailableSpace { get; set; } = string.Empty;
    public decimal? AreaSqFt { get; set; }
    public decimal? LengthFt { get; set; }
    public decimal? WidthFt { get; set; }
    public decimal? CalculatedRate { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string SpaceRequirement { get; set; } = string.Empty;
    public string OtherInformation { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RequiredDuration { get; set; } = string.Empty;
    public string RequiredTime { get; set; } = string.Empty;
    public bool ElectricityRequired { get; set; }
    public bool WaterRequired { get; set; }
    public string OtherFacilities { get; set; } = string.Empty;
    public string WasteManagement { get; set; } = string.Empty;
    public bool DeclarationAccepted { get; set; }
    public decimal? FeeAmount { get; set; }
    public string PaymentStatus { get; set; } = "Fee Pending";
    public DemandApplicationStatus Status { get; set; } = DemandApplicationStatus.Submitted;
    public DateTime? SubmittedAt { get; set; }
    public ICollection<DemandApplicationDocument> Documents { get; set; } = new List<DemandApplicationDocument>();
}

public class DemandApplicationDocument : BaseEntity
{
    public int DemandApplicationId { get; set; }
    public DemandApplication DemandApplication { get; set; } = null!;
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string VerificationStatus { get; set; } = "Unchecked";
    public string? RequestRemark { get; set; }
    public DateTime? RequestedAt { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? RequestTokenHash { get; set; }
    public DateTime? RequestTokenCreatedAt { get; set; }
    public DateTime? RequestTokenConsumedAt { get; set; }
}
