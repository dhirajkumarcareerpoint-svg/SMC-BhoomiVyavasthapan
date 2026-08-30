using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// कोणत्याही entity शी जोडलेले अपलोड केलेले दस्तऐवज (Documents).
/// </summary>
public class Document : BaseEntity
{
    public DocumentEntityType EntityType { get; set; }
    public int EntityId { get; set; }                    // संबंधित Property/Lease/... चा Id

    public string FileName { get; set; } = string.Empty;         // मूळ फाईल नाव
    public string StoredFileName { get; set; } = string.Empty;   // डिस्कवर साठवलेले सुरक्षित नाव
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public int? PropertyId { get; set; }
    public Property? Property { get; set; }
    public int? LeaseId { get; set; }
    public Lease? Lease { get; set; }
    public int? RecoveryCaseId { get; set; }
    public RecoveryCase? RecoveryCase { get; set; }
    public int? SchemeApplicationId { get; set; }
    public SchemeApplication? SchemeApplication { get; set; }
    public int? AllocationProcessId { get; set; }
    public AllocationProcess? AllocationProcess { get; set; }
    public int? CalculationId { get; set; }
    public Calculation? Calculation { get; set; }
}
