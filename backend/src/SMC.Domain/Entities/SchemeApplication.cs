using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// विविध उपक्रम (Tab 4): अभय योजना, दंडमाफी, सवलत, इतर महसूलवाढीचे उपक्रम.
/// </summary>
public class SchemeApplication : BaseEntity
{
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public SchemeType SchemeType { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string? ApplicantMobile { get; set; }
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public decimal OriginalOutstanding { get; set; }             // मूळ थकबाकी
    public decimal WaivedAmount { get; set; }                    // माफ केलेली रक्कम
    public decimal PayableAmount { get; set; }                   // भरावयाची रक्कम

    public SchemeStatus Status { get; set; } = SchemeStatus.Prapt;
    public DateTime? DecisionDate { get; set; }
    public string? ApprovedBy { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
