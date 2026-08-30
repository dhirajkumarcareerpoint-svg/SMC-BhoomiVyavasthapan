namespace SMC.Application.DTOs;

public class DocumentDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class AuditLogDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalProperties { get; set; }          // एकूण मालमत्ता
    public int TotalShops { get; set; }                // एकूण गाळे
    public int VacantProperties { get; set; }          // रिक्त मालमत्ता
    public int LeasedProperties { get; set; }          // भाडेतत्त्वावर दिलेल्या
    public decimal AnnualDemand { get; set; }          // वार्षिक मागणी
    public decimal TotalCollection { get; set; }       // एकूण वसुली
    public decimal TotalOutstanding { get; set; }      // एकूण थकबाकी
    public int PendingRecoveryCases { get; set; }      // प्रलंबित प्रकरणे
    public int SealedProperties { get; set; }
    public List<CategoryCountDto> CategoryBreakdown { get; set; } = new();
    public List<MonthlyCollectionDto> MonthlyCollection { get; set; } = new();
}

public class CategoryCountDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal AnnualDemand { get; set; }
}

public class MonthlyCollectionDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
