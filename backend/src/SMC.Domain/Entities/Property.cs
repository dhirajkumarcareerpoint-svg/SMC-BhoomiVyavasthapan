using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// मालमत्ता (Tab 1). Major गाळे, Mini गाळे, Land Fee, समाज मंदिर, अभ्यासिका, 256 गाळे,
/// TP-3/23, अधिकृत खोके, इतर भाडेतत्त्वावरील मनपा मालमत्ता — सर्व एकाच tabla मध्ये
/// PropertyCategory द्वारे विभागलेले.
/// </summary>
public class Property : BaseEntity
{
    public PropertyCategory Category { get; set; }
    public string PropertyCode { get; set; } = string.Empty;   // मालमत्ता क्रमांक / गाळा क्रमांक
    public string Name { get; set; } = string.Empty;           // मालमत्तेचे नाव / वर्णन
    public string? Ward { get; set; }                          // प्रभाग
    public string? Zone { get; set; }                          // झोन
    public string? Address { get; set; }                       // पत्ता / स्थान
    public decimal? AreaSqFt { get; set; }                     // क्षेत्रफळ (चौ.फूट)
    public decimal MonthlyRent { get; set; }                   // मासिक भाडे
    public decimal AnnualDemand { get; set; }                  // वार्षिक मागणी
    public string? SurveyNumber { get; set; }                  // सर्वे / गट क्रमांक
    public string? TpNumber { get; set; }                      // TP क्रमांक (TP-3/23 विभागासाठी)
    public PropertyStatus Status { get; set; } = PropertyStatus.Rikamy;
    public string? CurrentOccupant { get; set; }                // सध्याचा भाडेकरू / धारक

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
    public ICollection<RecoveryCase> RecoveryCases { get; set; } = new List<RecoveryCase>();
    public ICollection<SchemeApplication> SchemeApplications { get; set; } = new List<SchemeApplication>();
    public ICollection<AllocationProcess> AllocationProcesses { get; set; } = new List<AllocationProcess>();
    public ICollection<Calculation> Calculations { get; set; } = new List<Calculation>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
