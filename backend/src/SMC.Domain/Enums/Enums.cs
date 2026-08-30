namespace SMC.Domain.Enums;

public enum UserRole
{
    Admin = 1,      // प्रशासक - पूर्ण अधिकार
    Officer = 2,    // अधिकारी - मान्यता व संपादन अधिकार
    Staff = 3,      // कर्मचारी - नोंद व माहिती भरणे
    JE = 4,
    OS = 5,
    AssistantCommissioner = 6
}

/// <summary> मालमत्ता विभाग अंतर्गत sections </summary>
public enum PropertyCategory
{
    MajorGaale = 1,                   // Major गाळे
    MiniGaale = 2,                    // Mini गाळे
    LandFee = 3,                      // Land Fee (भुई भाडे)
    SamajMandir = 4,                  // समाज मंदिर
    Abhyasika = 5,                    // अभ्यासिका
    Gaale256 = 6,                     // 256 गाळे
    TP3_23 = 7,                       // TP-3/23
    AdhikrutKhoke = 8,                // अधिकृत खोके
    ItarBhadetatvavarilMalmatta = 9   // इतर भाडेतत्त्वावरील मनपा मालमत्ता
}

public enum PropertyStatus
{
    Rikamy = 1,       // रिक्त (Vacant)
    Bhadyane = 2,     // भाडेतत्त्वावर दिलेली (Leased)
    Seal = 3,         // सील केलेली
    Punarlilaw = 4,   // पुनर्लिलावासाठी
    Nishkriya = 5     // निष्क्रिय / बंद
}

/// <summary> हस्तांतरण - भाडेपट्टा कालावधी प्रकार </summary>
public enum LeaseDurationType
{
    Min3Years = 1,             // किमान 3 वर्षे
    ThreeToTenYears = 2,       // 3 ते 10 वर्षे
    Max29Years11Months = 3     // कमाल 29 वर्षे 11 महिने
}

public enum LeaseStatus
{
    Saru = 1,     // सुरू
    Sampla = 2,   // संपलेला
    Radd = 3      // रद्द
}

/// <summary> वसुली प्रक्रियेचे टप्पे </summary>
public enum RecoveryStage
{
    ThakbakiOlkhli = 1,   // थकबाकी ओळखली (>=3 महिने थकीत)
    NoticeDili = 2,       // नोटीस दिली
    VasuliSuru = 3,       // भाडे वसुली सुरू
    Seal = 4,             // सील
    Punarlilaw = 5,       // पुनर्लिलाव
    Band = 6              // प्रकरण बंद / वसूल
}

/// <summary> विविध उपक्रम प्रकार </summary>
public enum SchemeType
{
    AbhayYojana = 1,   // अभय योजना
    DandMafi = 2,      // दंडमाफी
    Savlat = 3,        // सवलत
    Itar = 4           // इतर महसूलवाढीचे उपक्रम
}

public enum SchemeStatus
{
    Prapt = 1,        // अर्ज प्राप्त
    ManjurZala = 2,   // मंजूर
    Naklat = 3        // नाकारले
}

/// <summary> मालमत्ता देण्याची कार्यपद्धती </summary>
public enum AllocationMethod
{
    SarvajanikLilaw = 1,     // सार्वजनिक लिलाव
    Niviva = 2,              // निविदा मागविणे
    PrasiddhikaranArj = 3    // प्रसिद्धीकरण करून अर्ज मागविणे
}

public enum AllocationStatus
{
    JahirNamaPrasiddh = 1,   // जाहीरनामा प्रसिद्ध
    ArjSwikarane = 2,        // अर्ज स्वीकारणे सुरू
    LilawZala = 3,           // लिलाव / निविदा पूर्ण
    Manjur = 4,              // मंजूर / वाटप पूर्ण
    Radd = 5                 // रद्द
}

public enum DocumentEntityType
{
    Property = 1,
    Lease = 2,
    RecoveryCase = 3,
    Scheme = 4,
    Allocation = 5,
    Calculation = 6
}

/// <summary>
/// गणना (Calculation) नोंदीची स्थिती.
/// टीप: सध्या रक्कम-गणनेचे नेमके व्यवसाय सूत्र (business formula) प्रणालीमध्ये निश्चित/उपलब्ध
/// नाही — त्यामुळे रक्कम अधिकाऱ्याने पडताळून भरावी लागते. सूत्र निश्चित झाल्यावर स्वयंचलित
/// आकारणी (auto-calculation) पुढील टप्प्यात जोडता येईल.
/// </summary>
public enum CalculationStatus
{
    Prarup = 1,     // प्रारूप (Draft) - अद्याप अंतिम नाही
    Nishchit = 2,   // निश्चित केलेली (Finalized/Confirmed)
    Radd = 3        // रद्द
}
