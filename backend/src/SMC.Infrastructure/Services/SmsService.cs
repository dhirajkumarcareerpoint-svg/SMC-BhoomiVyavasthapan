using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMC.Application.Interfaces;

namespace SMC.Infrastructure.Services;

public sealed class SmsOptions
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Log";
    public AclSmsOptions Acl { get; set; } = new();
}

public sealed class AclSmsOptions
{
    public string BaseUrl { get; set; } = "https://push3.aclgateway.com/servlet/com.aclwireless.pushconnectivity.listeners.TextListener";
    public string? AppId { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public string Sender { get; set; } = "MAHGOV";
    public string DltTemplateId { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "91";
    public int ContentType { get; set; } = 1;
    public int Alert { get; set; } = 1;
    public bool SelfId { get; set; } = true;
    public bool DlrReq { get; set; } = true;
}

public sealed class SmsService : ISmsService
{
    private readonly IHttpClientFactory _clients;
    private readonly SmsOptions _options;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IHttpClientFactory clients, IOptions<SmsOptions> options, ILogger<SmsService> logger)
    {
        _clients = clients;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string mobile, string message, string? dltTemplateId = null, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !string.Equals(_options.Provider, "ACL", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SMS was not sent because the SMS integration is disabled or not configured for ACL. Recipient suffix: {MobileSuffix}.", MaskMobile(mobile));
            return;
        }
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 12 && digits.StartsWith(_options.Acl.CountryCode, StringComparison.Ordinal)) digits = digits[2..];
        if (digits.Length != 10 || digits[0] is < '6' or > '9') throw new InvalidOperationException("Invalid SMS mobile number.");
        if (string.IsNullOrWhiteSpace(_options.Acl.AppId) || string.IsNullOrWhiteSpace(_options.Acl.UserId) || string.IsNullOrWhiteSpace(_options.Acl.Password)) throw new InvalidOperationException("ACL SMS credentials are not configured.");

        var parameters = new Dictionary<string, string?>
        {
            ["appid"] = _options.Acl.AppId,
            ["userId"] = _options.Acl.UserId,
            ["pass"] = _options.Acl.Password,
            ["contenttype"] = _options.Acl.ContentType.ToString(),
            ["from"] = _options.Acl.Sender,
            ["to"] = _options.Acl.CountryCode + digits,
            ["text"] = message,
            ["alert"] = _options.Acl.Alert.ToString(),
            ["selfid"] = _options.Acl.SelfId.ToString().ToLowerInvariant(),
            ["dlrreq"] = _options.Acl.DlrReq.ToString().ToLowerInvariant(),
            ["dtm"] = string.IsNullOrWhiteSpace(dltTemplateId) ? _options.Acl.DltTemplateId : dltTemplateId
        };
        var url = _options.Acl.BaseUrl + "?" + string.Join("&", parameters.Select(item => Uri.EscapeDataString(item.Key) + "=" + Uri.EscapeDataString(item.Value ?? string.Empty)));
        try
        {
            using var response = await _clients.CreateClient("AclSms").GetAsync(url, cancellationToken);
            var providerResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ACL SMS gateway returned HTTP {StatusCode} for recipient suffix {MobileSuffix}. Provider response: {ProviderResponse}", (int)response.StatusCode, digits[^4..], providerResponse);
            }
            else
            {
                _logger.LogInformation("ACL SMS gateway accepted application SMS for recipient suffix {MobileSuffix}. Message: {Message}. Provider response: {ProviderResponse}", digits[^4..], message, providerResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ACL SMS gateway request failed for mobile ending {MobileSuffix}.", digits[^4..]);
        }
    }

    private static string MaskMobile(string? mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length >= 4 ? $"****{digits[^4..]}" : "unknown";
    }
}