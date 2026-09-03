namespace SMC.Application.Interfaces;

public interface ISmsService
{
    Task SendAsync(string eventType, string applicationNumber, string message, string? recipientMobile = null, CancellationToken cancellationToken = default);
}

public static class SmsTemplateEvents
{
    public const string ApplicationSubmitted = nameof(ApplicationSubmitted);
    public const string PaymentRequired = nameof(PaymentRequired);
    public const string AssistantCommissionerNotification = nameof(AssistantCommissionerNotification);
    public const string ApplicationApproved = nameof(ApplicationApproved);
}

public static class SmsTemplateMessages
{
    public static string ApplicationSubmitted(string serviceType, string applicationNumber) =>
        $"आपण {serviceType} साठी अर्ज सादर केला आहे. आपल्या अर्जाचा क्रमांक {applicationNumber} आहे. कृपया भविष्यातील कार्यवाहीसाठी अर्ज क्रमांक जतन करून ठेवा. - SMC Solapur - MAHGOV";

    public static string PaymentRequired(string applicationNumber, string amount, string urlPartOne, string urlPartTwo) =>
        $"आपल्या अर्जाची प्राथमिक तपासणी पूर्ण झाली आहे. आपल्या अर्जाचा क्रमांक {applicationNumber} असून भरणा करावयाची रक्कम रु. {amount}  आहे. शुल्क भरण्यासाठी खालील लिंकवर क्लिक करा: {urlPartOne} {urlPartTwo} SMC Solapur - MAHGOV";

    public static string AssistantCommissionerNotification(string applicationNumber) =>
        $"अर्ज क्रमांक {applicationNumber} हा पुढिल कार्यवाहीसाठी प्राप्त झाला आहे. कृपया पुढील आवश्यक कार्यवाही करावी. - SMC Solapur - MAHGOV";
}
