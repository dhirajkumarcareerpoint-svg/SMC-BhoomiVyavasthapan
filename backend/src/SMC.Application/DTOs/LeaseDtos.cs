namespace SMC.Application.DTOs;

public class LeaseDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyCode { get; set; }
    public string LesseeName { get; set; } = string.Empty;
    public string? LesseeMobile { get; set; }
    public string? LesseeAddress { get; set; }
    public string DeedNumber { get; set; } = string.Empty;
    public DateTime DeedDate { get; set; }
    public string DurationType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RentAmount { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Shera { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateLeaseDto
{
    public int PropertyId { get; set; }
    public string LesseeName { get; set; } = string.Empty;
    public string? LesseeMobile { get; set; }
    public string? LesseeAddress { get; set; }
    public string DeedNumber { get; set; } = string.Empty;
    public DateTime DeedDate { get; set; }
    public string DurationType { get; set; } = "Min3Years";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RentAmount { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public string Status { get; set; } = "Saru";
    public string? Shera { get; set; }
}

public class UpdateLeaseDto : CreateLeaseDto { }
