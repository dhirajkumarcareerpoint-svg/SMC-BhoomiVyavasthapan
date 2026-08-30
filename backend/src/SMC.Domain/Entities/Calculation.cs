using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// Calculation (नवीन विभाग): निवडलेल्या मालमत्तेसाठी भाडे/शुल्क आकारणीची नोंद.
///
/// महत्त्वाची टीप (Business rule pending confirmation):
/// या प्रणालीमध्ये आकारणीचे नेमके व्यवसाय सूत्र (उदा. दर x कालावधी, मागील थकबाकी, चालू मागणी
/// यांचे नेमके गणिती संबंध) अद्याप निश्चित/दस्तऐवजीकरण केलेले नाही. त्यामुळे CalculatedAmount व
/// TotalAmount ही फील्ड्स सध्या अधिकाऱ्याने स्वहस्ते भरावयाची (मालमत्तेच्या Rate/MonthlyRent
/// आणि इतर संदर्भ माहितीच्या आधारे) आहेत. व्यवसाय सूत्र निश्चित झाल्यावर स्वयंचलित आकारणी लॉजिक
/// CalculationService मध्ये जोडता येईल — रचना (structure) त्यासाठी तयार ठेवली आहे.
/// </summary>
public class Calculation : BaseEntity
{
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    /// <summary>लागू दर (₹) - मालमत्तेच्या मासिक भाड्यावरून सुचवले जाते, आवश्यकतेनुसार संपादनीय.</summary>
    public decimal? Rate { get; set; }

    /// <summary>गणना कालावधी (महिने).</summary>
    public int PeriodMonths { get; set; }

    /// <summary>मागील थकबाकी (₹) - लागू असल्यास.</summary>
    public decimal? PreviousOutstanding { get; set; }

    /// <summary>चालू मागणी (₹) - लागू असल्यास.</summary>
    public decimal? CurrentDemand { get; set; }

    /// <summary>गणना केलेली रक्कम (₹) - सूत्र निश्चित होईपर्यंत स्वहस्ते भरावी.</summary>
    public decimal CalculatedAmount { get; set; }

    /// <summary>एकूण रक्कम (₹) - सूत्र निश्चित होईपर्यंत स्वहस्ते भरावी.</summary>
    public decimal TotalAmount { get; set; }

    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

    public CalculationStatus Status { get; set; } = CalculationStatus.Prarup;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
