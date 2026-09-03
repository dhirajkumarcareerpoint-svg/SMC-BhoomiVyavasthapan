using SMC.Application.Interfaces;
using SMC.Infrastructure.Services;

static void AssertEqual(string name, string expected, string actual)
{
    if (!string.Equals(expected, actual, StringComparison.Ordinal))
        throw new InvalidOperationException($"{name} failed.\nEXPECTED: {expected}\nACTUAL:   {actual}");
    Console.WriteLine($"{name}: PASS");
}

const string applicationNumber = "20261234";
const string serviceType = "MiniGaala";
const string amount = "1250.50";
const string url = "https://portal.example/application-status?applicationNumber=20261234&token=ABC123";
const string urlPartOne = "https://portal.example";
const string urlPartTwo = "/application-status?applicationNumber=20261234&token=ABC123";

var applicationMessage = SmsTemplateMessages.ApplicationSubmitted(serviceType, applicationNumber);
AssertEqual("Application exact text",
    "आपण MiniGaala साठी अर्ज सादर केला आहे. आपल्या अर्जाचा क्रमांक 20261234 आहे. कृपया भविष्यातील कार्यवाहीसाठी अर्ज क्रमांक जतन करून ठेवा. - SMC Solapur - MAHGOV",
    applicationMessage);

AssertEqual("Payment exact text",
    "आपल्या अर्जाची प्राथमिक तपासणी पूर्ण झाली आहे. आपल्या अर्जाचा क्रमांक 20261234 असून भरणा करावयाची रक्कम रु. 1250.50  आहे. शुल्क भरण्यासाठी खालील लिंकवर क्लिक करा: https://portal.example /application-status?applicationNumber=20261234&token=ABC123 SMC Solapur - MAHGOV",
    SmsTemplateMessages.PaymentRequired(applicationNumber, amount, urlPartOne, urlPartTwo));

AssertEqual("Assistant Commissioner exact text",
    "अर्ज क्रमांक 20261234 हा पुढिल कार्यवाहीसाठी प्राप्त झाला आहे. कृपया पुढील आवश्यक कार्यवाही करावी. - SMC Solapur - MAHGOV",
    SmsTemplateMessages.AssistantCommissionerNotification(applicationNumber));

AssertEqual("URG/URO secure URL", url, urlPartOne + urlPartTwo);

var acl = new AclSmsOptions
{
    AppId = "test-app",
    UserId = "test-user",
    Password = "test-password"
};
var request = SmsService.BuildRequestUrl(acl, "9876543210", applicationMessage, "1777178815669425110");
var query = new Uri(request).Query.TrimStart('?').Split('&')
    .Select(pair => pair.Split('=', 2))
    .ToDictionary(pair => Uri.UnescapeDataString(pair[0]), pair => Uri.UnescapeDataString(pair[1]));
AssertEqual("ACL dtm", "1777178815669425110", query["dtm"]);
AssertEqual("ACL recipient", "919876543210", query["to"]);
AssertEqual("ACL Unicode round trip", applicationMessage, query["text"]);
