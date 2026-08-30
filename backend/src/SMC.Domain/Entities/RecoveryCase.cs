using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// वसुली प्रक्रिया (Tab 3): किमान 3 महिने भाडे थकीत → नोटीस → भाडे वसुली → सील → पुनर्लिलाव.
/// </summary>
public class RecoveryCase : BaseEntity
{
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public int? LeaseId { get; set; }
    public Lease? Lease { get; set; }

    public int MonthsOverdue { get; set; }                     // थकीत महिने (किमान 3)
    public decimal OutstandingAmount { get; set; }             // थकबाकी रक्कम

    public RecoveryStage Stage { get; set; } = RecoveryStage.ThakbakiOlkhli;

    public string? NoticeNumber { get; set; }                  // नोटीस क्रमांक
    public DateTime? NoticeDate { get; set; }                  // नोटीस तारीख

    public decimal RecoveredAmount { get; set; }                // वसूल झालेली रक्कम
    public DateTime? RecoveryDate { get; set; }

    public DateTime? SealDate { get; set; }                     // सील तारीख
    public DateTime? ReAuctionDate { get; set; }                 // पुनर्लिलाव तारीख

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
