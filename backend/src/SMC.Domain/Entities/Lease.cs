using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// हस्तांतरण (Tab 2) - दस्ताद्वारे भाडेपट्टा.
/// </summary>
public class Lease : BaseEntity
{
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public string LesseeName { get; set; } = string.Empty;     // भाडेकरू / धारकाचे नाव
    public string? LesseeMobile { get; set; }
    public string? LesseeAddress { get; set; }
    public string DeedNumber { get; set; } = string.Empty;     // दस्त क्रमांक
    public DateTime DeedDate { get; set; }                     // दस्त नोंदणी तारीख

    public LeaseDurationType DurationType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal RentAmount { get; set; }                    // भाडे रक्कम
    public decimal? SecurityDeposit { get; set; }               // अनामत रक्कम
    public LeaseStatus Status { get; set; } = LeaseStatus.Saru;

    public ICollection<RecoveryCase> RecoveryCases { get; set; } = new List<RecoveryCase>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
