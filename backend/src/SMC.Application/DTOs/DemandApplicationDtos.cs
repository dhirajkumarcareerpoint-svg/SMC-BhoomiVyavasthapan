using SMC.Domain.Enums;

namespace SMC.Application.DTOs;

public class DemandApplicationDto
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
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
    public bool ElectricityRequired { get; set; }
    public bool WaterRequired { get; set; }
    public string OtherFacilities { get; set; } = string.Empty;
    public string WasteManagement { get; set; } = string.Empty;
    public bool DeclarationAccepted { get; set; }
    public decimal? FeeAmount { get; set; }
    public string PaymentStatus { get; set; } = "Fee Pending";
    public DemandApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? WorkflowStage { get; set; }
    public string? WorkflowPaymentStatus { get; set; }
    public List<DemandApplicationDocumentDto> Documents { get; set; } = new();
}

public class DemandApplicationDocumentDto
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string VerificationStatus { get; set; } = "Unchecked";
    public string? RequestRemark { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? RequestToken { get; set; }
    public string? SecureRequestUrl { get; set; }
}

/// <summary>Returned only when an anonymous applicant creates an application.</summary>
public class PublicDemandApplicationSessionDto
{
    public DemandApplicationDto Application { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
}

public class CreateDemandApplicationDto
{
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
    public bool ElectricityRequired { get; set; }
    public bool WaterRequired { get; set; }
    public string OtherFacilities { get; set; } = string.Empty;
    public string WasteManagement { get; set; } = string.Empty;
    public bool DeclarationAccepted { get; set; }
    public decimal? FeeAmount { get; set; }
}
public class UpdateDemandApplicationDto : CreateDemandApplicationDto { }
public class DocumentVerificationDto { public string Status { get; set; } = string.Empty; public string? Remark { get; set; } }
