namespace SMC.Application.DTOs;

public class RecoveryCaseDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyCode { get; set; }
    public int? LeaseId { get; set; }
    public string? LesseeName { get; set; }
    public int MonthsOverdue { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string? NoticeNumber { get; set; }
    public DateTime? NoticeDate { get; set; }
    public decimal RecoveredAmount { get; set; }
    public DateTime? RecoveryDate { get; set; }
    public DateTime? SealDate { get; set; }
    public DateTime? ReAuctionDate { get; set; }
    public string? Shera { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRecoveryCaseDto
{
    public int PropertyId { get; set; }
    public int? LeaseId { get; set; }
    public int MonthsOverdue { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Stage { get; set; } = "ThakbakiOlkhli";
    public string? NoticeNumber { get; set; }
    public DateTime? NoticeDate { get; set; }
    public decimal RecoveredAmount { get; set; }
    public DateTime? RecoveryDate { get; set; }
    public DateTime? SealDate { get; set; }
    public DateTime? ReAuctionDate { get; set; }
    public string? Shera { get; set; }
}

public class UpdateRecoveryCaseDto : CreateRecoveryCaseDto { }
