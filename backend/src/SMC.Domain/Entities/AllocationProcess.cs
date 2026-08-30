using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// मालमत्ता देण्याची कार्यपद्धती (Tab 5): सार्वजनिक लिलाव, निविदा मागविणे,
/// प्रसिद्धीकरण करून अर्ज मागविणे.
/// </summary>
public class AllocationProcess : BaseEntity
{
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public AllocationMethod Method { get; set; }
    public string? NoticeNumber { get; set; }                   // जाहिरात / निविदा क्रमांक
    public DateTime PublishDate { get; set; }                   // प्रसिद्धी तारीख
    public DateTime? LastDateToApply { get; set; }               // अर्ज करण्याची अंतिम तारीख
    public DateTime? AuctionDate { get; set; }                    // लिलाव / निविदा उघडण्याची तारीख

    public decimal? ReserveAmount { get; set; }                  // राखीव किंमत
    public decimal? HighestBidAmount { get; set; }                // सर्वाधिक बोली रक्कम
    public string? HighestBidderName { get; set; }                // सर्वाधिक बोली लावणारा
    public string? HighestBidderMobile { get; set; }

    public AllocationStatus Status { get; set; } = AllocationStatus.JahirNamaPrasiddh;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
