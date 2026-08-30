namespace SMC.Application.DTOs;

public class PropertyDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string PropertyCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string? Zone { get; set; }
    public string? Address { get; set; }
    public decimal? AreaSqFt { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal AnnualDemand { get; set; }
    public string? SurveyNumber { get; set; }
    public string? TpNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentOccupant { get; set; }
    public string? Shera { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int DocumentCount { get; set; }
}

public class CreatePropertyDto
{
    public string Category { get; set; } = string.Empty;
    public string PropertyCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string? Zone { get; set; }
    public string? Address { get; set; }
    public decimal? AreaSqFt { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal AnnualDemand { get; set; }
    public string? SurveyNumber { get; set; }
    public string? TpNumber { get; set; }
    public string Status { get; set; } = "Rikamy";
    public string? CurrentOccupant { get; set; }
    public string? Shera { get; set; }
}

public class UpdatePropertyDto : CreatePropertyDto { }
