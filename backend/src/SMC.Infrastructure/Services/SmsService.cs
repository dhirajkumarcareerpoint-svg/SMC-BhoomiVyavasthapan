using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Services;

public sealed class SmsOptions
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Log";
    public string BaseUrl { get; set; } = "https://push3.aclgateway.com/servlet/com.aclwireless.pushconnectivity.listeners.TextListener";
    public string? AppId { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public string From { get; set; } = "MAHGOV";
    public string CountryCode { get; set; } = "91";
    public int ContentType { get; set; } = 1;
    public int Alert { get; set; } = 1;
    public bool SelfId { get; set; } = true;
    public bool DlrReq { get; set; } = true;
    public string? PublicBaseUrl { get; set; }
    public string? AssistantCommissionerMobile { get; set; }
}

public sealed class SmsService : ISmsService
{
    private readonly IApplicationDbContext _db; private readonly IHttpClientFactory _clients; private readonly SmsOptions _options; private readonly ILogger<SmsService> _logger;
    private static readonly Dictionary<string, (string Id, string Body)> Templates = new()
    {
        ["ApplicationSubmitted"] = ("1777178815669425110", "आपण {#alp#} साठी अर्ज सादर केला आहे. आपल्या अर्जाचा क्रमांक {#num#} आहे. कृपया भविष्यातील कार्यवाहीसाठी अर्ज क्रमांक जतन करून ठेवा. - SMC Solapur - MAHGOV"),
        ["PaymentRequired"] = ("1777178815696801613", "आपल्या अर्जाची प्राथमिक तपासणी पूर्ण झाली आहे. आपल्या अर्जाचा क्रमांक {#num#} असून भरणा करावयाची रक्कम रु. {#num#}  आहे. शुल्क भरण्यासाठी खालील लिंकवर क्लिक करा: {#urg#} {#uro#} SMC Solapur - MAHGOV"),
        ["AssistantCommissionerNotification"] = ("1777178815700976811", "अर्ज क्रमांक {#num#} हा पुढिल कार्यवाहीसाठी प्राप्त झाला आहे. कृपया पुढील आवश्यक कार्यवाही करावी. - SMC Solapur - MAHGOV")
    };
    public SmsService(IApplicationDbContext db, IHttpClientFactory clients, IOptions<SmsOptions> options, ILogger<SmsService> logger) => (_db, _clients, _options, _logger) = (db, clients, options.Value, logger);
    public async Task SendAsync(string mobile, string eventType, IReadOnlyDictionary<string, string?> values, string? applicationNumber = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber) || !Templates.TryGetValue(eventType, out var template)) return;
        if (await _db.SmsEvents.AnyAsync(x => x.ApplicationNumber == applicationNumber && x.EventType == eventType && !x.IsDeleted, cancellationToken)) return;
        var ev = new SmsEvent { RecipientMobile = Normalize(mobile, out var destination) ? destination : "Invalid", EventType = eventType, ApplicationNumber = applicationNumber, TemplateReference = template.Id, CreatedBy = "SmsService", Status = "Queued" };
        _db.SmsEvents.Add(ev); await _db.SaveChangesAsync(cancellationToken);
        if (!_options.Enabled || !string.Equals(_options.Provider, "ACL", StringComparison.OrdinalIgnoreCase)) { ev.Status = "Suppressed"; ev.FailureReason = "SMS disabled or Log provider selected."; await _db.SaveChangesAsync(cancellationToken); return; }
        if (!Normalize(mobile, out destination) || string.IsNullOrWhiteSpace(_options.AppId) || string.IsNullOrWhiteSpace(_options.UserId) || string.IsNullOrWhiteSpace(_options.Password)) { ev.Status = "Failed"; ev.FailureReason = "Invalid mobile number or incomplete ACL configuration."; await _db.SaveChangesAsync(cancellationToken); return; }
        var text = RenderTemplate(eventType, template.Body, values);
        try { using var response = await _clients.CreateClient("AclSms").GetAsync(BuildUrl(destination, text, template.Id), cancellationToken); var body = await response.Content.ReadAsStringAsync(cancellationToken); ev.Status = response.IsSuccessStatusCode ? "Sent" : "Failed"; ev.SentAt = response.IsSuccessStatusCode ? DateTime.UtcNow : null; ev.ProviderMessageId = body.Length > 250 ? body[..250] : body; ev.FailureReason = response.IsSuccessStatusCode ? null : $"ACL HTTP {(int)response.StatusCode}."; }
        catch (Exception ex) { ev.Status = "Failed"; ev.FailureReason = "ACL request failed."; _logger.LogWarning(ex, "ACL SMS failed for {EventType}, application {ApplicationNumber}", eventType, applicationNumber); }
        await _db.SaveChangesAsync(cancellationToken);
    }
    private Uri BuildUrl(string to, string text, string templateId)
    {
        var p = new Dictionary<string, string?> { ["appid"] = _options.AppId, ["userId"] = _options.UserId, ["pass"] = _options.Password, ["contenttype"] = _options.ContentType.ToString(), ["from"] = _options.From, ["to"] = to, ["text"] = text, ["alert"] = _options.Alert.ToString(), ["selfid"] = _options.SelfId.ToString().ToLowerInvariant(), ["dlrreq"] = _options.DlrReq.ToString().ToLowerInvariant(), ["dtm"] = templateId };
        return new Uri(_options.BaseUrl + "?" + string.Join("&", p.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value ?? ""))));
    }
    private static string RenderTemplate(string eventType, string template, IReadOnlyDictionary<string, string?> values)
    {
        if (eventType != "PaymentRequired")
            return template.Replace("{#alp#}", values.GetValueOrDefault("ServiceName") ?? "").Replace("{#num#}", values.GetValueOrDefault("ApplicationNumber") ?? "");

        var applicationNumber = values.GetValueOrDefault("ApplicationNumber") ?? string.Empty;
        var amount = values.GetValueOrDefault("Amount") ?? string.Empty;
        var paymentLink = values.GetValueOrDefault("PaymentLink") ?? string.Empty;
        var splitAt = paymentLink.LastIndexOf("token=", StringComparison.OrdinalIgnoreCase);
        var urlPrefix = splitAt >= 0 ? paymentLink[..(splitAt + "token=".Length)] : paymentLink;
        var urlRemainder = splitAt >= 0 ? paymentLink[(splitAt + "token=".Length)..] : string.Empty;
        var firstNumber = ReplaceFirst(template, "{#num#}", applicationNumber);
        var secondNumber = ReplaceFirst(firstNumber, "{#num#}", amount);
        return secondNumber.Replace("{#urg#}", urlPrefix).Replace("{#uro#}", urlRemainder);
    }
    private static string ReplaceFirst(string value, string token, string replacement)
    {
        var index = value.IndexOf(token, StringComparison.Ordinal);
        return index < 0 ? value : value[..index] + replacement + value[(index + token.Length)..];
    }
    private bool Normalize(string mobile, out string result) { var d = new string(mobile.Where(char.IsDigit).ToArray()); if (d.Length == 12 && d.StartsWith(_options.CountryCode)) d = d[2..]; if (d.Length != 10 || d[0] < '6' || d[0] > '9') { result = ""; return false; } result = _options.CountryCode + d; return true; }
}
