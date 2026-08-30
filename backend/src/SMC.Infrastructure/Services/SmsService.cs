using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;

namespace SMC.Infrastructure.Services;

/// <summary>Records rendered notification intents until an approved provider adapter is installed.</summary>
public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly IApplicationDbContext _db;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger, IApplicationDbContext db)
        => (_configuration, _logger, _db) = (configuration, logger, db);

    public async Task SendAsync(string mobile, string eventType, IReadOnlyDictionary<string, string?> values, string? applicationNumber = null, CancellationToken cancellationToken = default)
    {
        var template = await _db.SmsTemplates.AsNoTracking().Where(x => x.EventType == eventType && x.IsActive).OrderByDescending(x => x.Version).FirstOrDefaultAsync(cancellationToken);
        var enabled = _configuration.GetValue<bool>("Sms:Enabled");
        var provider = _configuration["Sms:Provider"];
        var templateMissing = template is null;
        var renderedMessage = templateMissing ? null : Render(template!, values);
        var smsEvent = new SmsEvent
        {
            RecipientMobile = mobile,
            EventType = eventType,
            ApplicationNumber = applicationNumber,
            TemplateReference = template is null ? null : $"{template.TemplateName}:v{template.Version}",
            CreatedBy = "SmsService",
            Status = templateMissing ? "TemplateMissing" : enabled && !string.IsNullOrWhiteSpace(provider) && !provider.Equals("Log", StringComparison.OrdinalIgnoreCase) ? "PendingProviderAdapter" : "Suppressed"
        };

        if (templateMissing)
        {
            smsEvent.FailureReason = $"No active SMS template is configured for event '{eventType}'.";
            _logger.LogWarning("SMS event {EventType} was not rendered because no active template exists.", eventType);
        }
        else if (!enabled || string.IsNullOrWhiteSpace(provider) || provider.Equals("Log", StringComparison.OrdinalIgnoreCase))
        {
            smsEvent.FailureReason = "SMS delivery disabled; development/mock event recorded.";
            _logger.LogInformation("SMS event {EventType} suppressed for {Mobile}, application {ApplicationNumber}: {Message}", eventType, mobile, applicationNumber, renderedMessage);
        }
        else
        {
            smsEvent.FailureReason = "No provider adapter installed. Configure an approved provider after DLT onboarding.";
            _logger.LogWarning("SMS event {EventType} queued for provider {Provider}; no adapter is installed.", eventType, provider);
        }

        _db.SmsEvents.Add(smsEvent);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string Render(SmsTemplate template, IReadOnlyDictionary<string, string?> values)
    {
        var mapping = JsonSerializer.Deserialize<List<string>>(template.VariableMapping) ?? [];
        var index = 0;
        return Regex.Replace(template.MessageBody, @"\{#VAR\}|\(#VAR\)", match =>
        {
            var variable = index < mapping.Count ? mapping[index] : null;
            index++;
            return variable is not null && values.TryGetValue(variable, out var value) && value is not null ? value : match.Value;
        });
    }
}
