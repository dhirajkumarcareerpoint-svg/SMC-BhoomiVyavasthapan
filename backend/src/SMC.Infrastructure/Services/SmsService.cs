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
    public string? AssistantCommissionerMobile { get; set; }
    public Dictionary<string, string> Templates { get; set; } = new(StringComparer.Ordinal);
    public AclSmsOptions Acl { get; set; } = new();
}

public sealed class AclSmsOptions
{
    public string BaseUrl { get; set; } = "https://push3.aclgateway.com/servlet/com.aclwireless.pushconnectivity.listeners.TextListener";
    public string? AppId { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public string Sender { get; set; } = "MAHGOV";
    public string CountryCode { get; set; } = "91";
    public int ContentType { get; set; } = 1;
    public int Alert { get; set; } = 1;
    public bool SelfId { get; set; } = true;
    public bool DlrReq { get; set; } = true;
}

public sealed class SmsService : ISmsService
{
    private readonly IHttpClientFactory _clients;
    private readonly IApplicationDbContext _db;
    private readonly SmsOptions _options;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IHttpClientFactory clients, IApplicationDbContext db, IOptions<SmsOptions> options, ILogger<SmsService> logger)
    {
        _clients = clients;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string eventType, string applicationNumber, string message, string? recipientMobile = null, CancellationToken cancellationToken = default)
    {
        var mobile = eventType == SmsTemplateEvents.AssistantCommissionerNotification
            ? _options.AssistantCommissionerMobile
            : recipientMobile;

        if (!_options.Enabled || !string.Equals(_options.Provider, "ACL", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SMS was not sent because the SMS integration is disabled or not configured for ACL. Event: {EventType}; recipient suffix: {MobileSuffix}.", eventType, MaskMobile(mobile));
            return;
        }

        SmsEvent? smsEvent = null;
        try
        {
            if (!_options.Templates.TryGetValue(eventType, out var dltTemplateId) || string.IsNullOrWhiteSpace(dltTemplateId))
                throw new InvalidOperationException($"DLT template ID is not configured for SMS event '{eventType}'.");

            if (await _db.SmsEvents.AnyAsync(x => x.ApplicationNumber == applicationNumber && x.EventType == eventType, cancellationToken))
            {
                _logger.LogInformation("Duplicate SMS event {EventType} for application {ApplicationNumber} was skipped.", eventType, applicationNumber);
                return;
            }

            var digits = NormalizeMobile(mobile);
            if (string.IsNullOrWhiteSpace(_options.Acl.AppId) || string.IsNullOrWhiteSpace(_options.Acl.UserId) || string.IsNullOrWhiteSpace(_options.Acl.Password))
                throw new InvalidOperationException("ACL SMS credentials are not configured.");

            smsEvent = new SmsEvent
            {
                RecipientMobile = digits,
                EventType = eventType,
                ApplicationNumber = applicationNumber,
                TemplateReference = dltTemplateId,
                Status = "Pending",
                CreatedBy = "System"
            };
            _db.SmsEvents.Add(smsEvent);
            await _db.SaveChangesAsync(cancellationToken);

            var url = BuildRequestUrl(_options.Acl, digits, message, dltTemplateId);
            using var response = await _clients.CreateClient("AclSms").GetAsync(url, cancellationToken);
            var providerResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            smsEvent.ProviderMessageId = Truncate(providerResponse, 200);
            smsEvent.Status = response.IsSuccessStatusCode ? "Sent" : "Failed";
            smsEvent.SentAt = response.IsSuccessStatusCode ? DateTime.UtcNow : null;
            smsEvent.FailureReason = response.IsSuccessStatusCode ? null : Truncate($"HTTP {(int)response.StatusCode}: {providerResponse}", 1000);
            smsEvent.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("ACL SMS gateway returned HTTP {StatusCode} for event {EventType}, recipient suffix {MobileSuffix}. Provider response: {ProviderResponse}", (int)response.StatusCode, eventType, MaskMobile(digits), providerResponse);
            else
                _logger.LogInformation("ACL SMS gateway accepted event {EventType} for recipient suffix {MobileSuffix}. Provider response: {ProviderResponse}", eventType, MaskMobile(digits), providerResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMS event {EventType} failed for application {ApplicationNumber}; the completed workflow action is unchanged.", eventType, applicationNumber);
            if (smsEvent is not null)
            {
                try
                {
                    smsEvent.Status = "Failed";
                    smsEvent.FailureReason = Truncate(ex.Message, 1000);
                    smsEvent.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch (Exception persistenceException)
                {
                    _logger.LogWarning(persistenceException, "Could not persist failure state for SMS event {EventType}.", eventType);
                }
            }
        }
    }

    private string NormalizeMobile(string? mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith(_options.Acl.CountryCode, StringComparison.Ordinal) && digits.Length == _options.Acl.CountryCode.Length + 10)
            digits = digits[_options.Acl.CountryCode.Length..];
        if (digits.Length != 10 || digits[0] is < '6' or > '9') throw new InvalidOperationException("Invalid SMS mobile number.");
        return digits;
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    public static string BuildRequestUrl(AclSmsOptions options, string tenDigitMobile, string message, string dltTemplateId)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["appid"] = options.AppId,
            ["userId"] = options.UserId,
            ["pass"] = options.Password,
            ["contenttype"] = options.ContentType.ToString(),
            ["from"] = options.Sender,
            ["to"] = options.CountryCode + tenDigitMobile,
            ["text"] = message,
            ["alert"] = options.Alert.ToString(),
            ["selfid"] = options.SelfId.ToString().ToLowerInvariant(),
            ["dlrreq"] = options.DlrReq.ToString().ToLowerInvariant(),
            ["dtm"] = dltTemplateId
        };
        return options.BaseUrl + "?" + string.Join("&", parameters.Select(item => Uri.EscapeDataString(item.Key) + "=" + Uri.EscapeDataString(item.Value ?? string.Empty)));
    }

    private static string MaskMobile(string? mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length >= 4 ? $"****{digits[^4..]}" : "unknown";
    }
}
