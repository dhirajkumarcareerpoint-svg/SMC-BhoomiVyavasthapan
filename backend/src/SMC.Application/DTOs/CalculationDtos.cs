namespace SMC.Application.DTOs;

public class CalculationDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyCode { get; set; }
    public string? PropertyCategory { get; set; }
    public decimal? AreaSqFt { get; set; }
    public decimal? Rate { get; set; }
    public int PeriodMonths { get; set; }
    public decimal? PreviousOutstanding { get; set; }
    public decimal? CurrentDemand { get; set; }
    public decimal CalculatedAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CalculationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Shera { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCalculationDto
{
    public int PropertyId { get; set; }
    public decimal? Rate { get; set; }
    public int PeriodMonths { get; set; }
    public decimal? PreviousOutstanding { get; set; }
    public decimal? CurrentDemand { get; set; }
    public decimal CalculatedAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Prarup";
    public string? Shera { get; set; }
}

public class UpdateCalculationDto : CreateCalculationDto { }
