using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public interface IDemandWorkflowService
{
    Task<DemandWorkflowDto?> GetAsync(int applicationId);
    Task<PublicPaymentDto?> GetPublicPaymentAsync(string applicationNumber, string accessToken);
    Task<PublicDemandApplicationStatusDto?> GetPublicStatusAsync(string applicationNumber, string? paymentAccessToken, string? requestToken = null);
    Task<List<DemandWorkflowDto>> QueueAsync(string actor);
    Task<List<ProcessedDemandWorkflowDto>> ProcessedHistoryAsync(string actor);
    Task<DemandWorkflowDto> EnsureAsync(int applicationId, string actor);
    Task<DemandWorkflowDto> VerifyJeAsync(int applicationId, string actor, bool approve, string? reason);
    Task<DemandWorkflowDto> VerifyOsAsync(int applicationId, string actor, bool approve, string? reason);
    Task<DemandWorkflowDto> CreatePaymentRequestAsync(int applicationId, string actor, decimal payableAmount);
    Task<DemandWorkflowDto> SubmitPaymentAsync(int applicationId, PaymentConfirmationDto dto, Stream screenshot, string fileName, string contentType, string accessToken);
    Task<DemandWorkflowDto> VerifyPaymentAsync(int applicationId, string actor, bool approve, string? reason);
    Task<DemandWorkflowDto> SetPaymentStatusAsync(int applicationId, string actor, string paymentStatus);
    Task<DemandWorkflowDto> ForwardToAssistantCommissionerAsync(int applicationId, string actor);
    Task<DemandWorkflowDto> ApproveAsync(int applicationId, string actor);
    Task<DemandWorkflowDto> RejectFinalAsync(int applicationId, string actor, string? reason);
    Task<(byte[] content, string fileName)> GenerateApplicationPdfAsync(int applicationId);
}

public class DemandWorkflowService : IDemandWorkflowService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly IFileStorageService _storage;
    private readonly ISmsService _sms;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DemandWorkflowService> _logger;

    public DemandWorkflowService(IApplicationDbContext db, IAuditService audit, IFileStorageService storage, ISmsService sms, ICurrentUserService currentUser, ILogger<DemandWorkflowService> logger)
    { _db = db; _audit = audit; _storage = storage; _sms = sms; _currentUser = currentUser; _logger = logger; }

    public async Task<DemandWorkflowDto?> GetAsync(int applicationId) => await FindDto(applicationId);

    public async Task<PublicPaymentDto?> GetPublicPaymentAsync(string applicationNumber, string accessToken)
    {
        var workflow = await PublicWorkflow(applicationNumber, accessToken);
        return workflow is null ? null : new PublicPaymentDto
        {
            DemandApplicationId = workflow.DemandApplicationId,
            ApplicationNumber = workflow.DemandApplication.ApplicationNumber,
            ApplicantName = workflow.DemandApplication.ApplicantName,
            PayableAmount = workflow.PayableAmount,
            Stage = workflow.Stage,
            PaymentStatus = workflow.PaymentStatus,
            PaymentDate = workflow.PaymentDate,
            CertificateFileName = workflow.CertificateFileName,
            CertificateFilePath = workflow.CertificateFilePath
        };
    }

    public async Task<PublicDemandApplicationStatusDto?> GetPublicStatusAsync(string applicationNumber, string? paymentAccessToken, string? requestToken = null)
    {
        var number = applicationNumber?.Trim();
        if (string.IsNullOrWhiteSpace(number)) return null;
        var workflow = await _db.DemandApplicationWorkflows.AsNoTracking().Include(item => item.DemandApplication).ThenInclude(application => application.Documents)
            .FirstOrDefaultAsync(item => item.DemandApplication.ApplicationNumber == number && !item.DemandApplication.IsDeleted);
        if (workflow is null) return null;
        var actions = await _db.AuditLogs.AsNoTracking().Where(log => log.EntityName == nameof(DemandApplication) && log.EntityId == workflow.DemandApplicationId)
            .OrderByDescending(log => log.Timestamp).ToListAsync();
        var app = workflow.DemandApplication;
        AuditLog? Latest(params string[] names) => actions.FirstOrDefault(log => names.Contains(log.Action));
        var je = Latest("JE Verified", "JE Rejected"); var os = Latest("Forwarded to Assistant Commissioner", "Payment Request Sent", "OS Rejected", "Payment Verified", "Payment Rejected"); var ac = Latest("Final Approved", "Assistant Commissioner Rejected");
        string Status(AuditLog? action, string pendingStage, string approvedAction, string rejectedAction, string fallback) => action?.Action == approvedAction ? "Approved" : action?.Action == rejectedAction ? "Rejected" : workflow.Stage == pendingStage ? "Pending" : fallback;
        var request = app.Documents.Where(document => !document.IsDeleted && document.RequestedAt != null).OrderByDescending(document => document.RequestedAt).FirstOrDefault();
        var canResubmit = request is not null && request.RequestTokenConsumedAt is null && !string.IsNullOrWhiteSpace(requestToken) && CryptographicOperations.FixedTimeEquals(Convert.FromHexString(request.RequestTokenHash ?? "00"), SHA256.HashData(Encoding.UTF8.GetBytes(requestToken)));
        return new PublicDemandApplicationStatusDto
        {
            DemandApplicationId = app.Id, ApplicationNumber = app.ApplicationNumber, ApplicantName = app.ApplicantName, SubmittedAt = app.SubmittedAt, CurrentStatus = workflow.Stage, PayableAmount = workflow.PayableAmount, PaymentStatus = workflow.PaymentStatus,
            PaymentAccessGranted = workflow.Stage == "PaymentRequired"
                && workflow.PaymentStatus == "PaymentRequired"
                && FixedTimeEquals(workflow.PaymentAccessToken, paymentAccessToken), HasDocumentRequest = request is not null, RequestedDocumentId=request?.Id, RequestedDocumentType=request?.DocumentType, RequestedDocumentName=request?.FileName, RequestRemark=request?.RequestRemark, RequestDate=request?.RequestedAt, RequestStatus=request?.VerificationStatus, CanResubmitRequestedDocument=canResubmit,
            Je = new PublicWorkflowLevelDto { Status = je?.Action == "JE Verified" ? "Accepted" : je?.Action == "JE Rejected" ? "Rejected" : workflow.Stage == "JEPending" ? "Pending" : "Accepted", ActionAt = je?.Timestamp, RejectionReason = je?.Action == "JE Rejected" ? workflow.RejectionReason : null },
            Os = new PublicWorkflowLevelDto { Status = os?.Action == "Forwarded to Assistant Commissioner" || workflow.Stage is "AssistantCommissionerApprovalPending" or "Approved" ? "Forwarded" : os?.Action == "Payment Request Sent" ? "Payment Required" : os?.Action is "OS Rejected" or "Payment Rejected" ? "Rejected" : workflow.Stage == "OSPending" ? "Pending" : "Accepted", ActionAt = os?.Timestamp, RejectionReason = os?.Action is "OS Rejected" or "Payment Rejected" ? workflow.RejectionReason : null, PaymentStatus = workflow.PaymentStatus },
            AssistantCommissioner = new PublicWorkflowLevelDto { Status = Status(ac, "AssistantCommissionerApprovalPending", "Final Approved", "Assistant Commissioner Rejected", "Pending"), ActionAt = ac?.Timestamp, RejectionReason = ac?.Action == "Assistant Commissioner Rejected" ? workflow.RejectionReason : null }
        };
    }

    public async Task<List<DemandWorkflowDto>> QueueAsync(string actor)
    {
        var role = RoleFor(actor);
        var stages = role switch
        {
            "JE" => new[] { "JEPending" },
            "OS" => new[] { "OSPending", "PaymentRequired", "PaymentVerificationPending" },
            "AssistantCommissioner" => new[] { "AssistantCommissionerApprovalPending" },
            _ => Array.Empty<string>()
        };
        var rows = await _db.DemandApplicationWorkflows.Include(x => x.DemandApplication).Where(x => stages.Contains(x.Stage)).OrderBy(x => x.CreatedAt).ToListAsync();
        return rows.Select(ToDto).ToList();
    }

    public async Task<List<ProcessedDemandWorkflowDto>> ProcessedHistoryAsync(string actor)
    {
        var actions = RoleFor(actor) switch
        {
            "JE" => new[] { "JE Verified", "JE Rejected" },
            "OS" => new[] { "Payment Request Sent", "OS Rejected", "Payment Verified", "Payment Rejected", "Payment status set to PaymentPending", "Payment status set to PaymentDone", "Forwarded to Assistant Commissioner" },
            "AssistantCommissioner" => new[] { "Final Approved", "Assistant Commissioner Rejected" },
            _ => Array.Empty<string>()
        };
        if (actions.Length == 0) return [];

        var actionsByOfficer = await _db.AuditLogs.AsNoTracking()
            .Where(log => log.EntityName == nameof(DemandApplication) && log.UserName == actor && actions.Contains(log.Action))
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
        var latestActions = actionsByOfficer.GroupBy(log => log.EntityId).Select(group => group.First()).ToList();
        if (latestActions.Count == 0) return [];

        var applicationIds = latestActions.Select(log => log.EntityId).ToList();
        var workflows = await _db.DemandApplicationWorkflows.AsNoTracking()
            .Include(workflow => workflow.DemandApplication)
            .Where(workflow => applicationIds.Contains(workflow.DemandApplicationId))
            .ToDictionaryAsync(workflow => workflow.DemandApplicationId);

        return latestActions
            .Where(log => workflows.ContainsKey(log.EntityId))
            .Select(log => new ProcessedDemandWorkflowDto
            {
                Workflow = ToDto(workflows[log.EntityId]),
                Action = log.Action,
                ActionAt = log.Timestamp
            })
            .ToList();
    }

    public async Task<DemandWorkflowDto> EnsureAsync(int applicationId, string actor)
    {
        var application = await _db.DemandApplications.FirstOrDefaultAsync(x => x.Id == applicationId && !x.IsDeleted) ?? throw new InvalidOperationException("अर्ज सापडला नाही.");
        var workflow = await _db.DemandApplicationWorkflows.FirstOrDefaultAsync(x => x.DemandApplicationId == applicationId);
        if (workflow is null)
        {
            application.FeeAmount = null;
            application.PaymentStatus = "PendingOSAmount";
            workflow = new DemandApplicationWorkflow { DemandApplicationId = applicationId, Stage = "JEPending", PayableAmount = 0m, CreatedBy = actor };
            _db.DemandApplicationWorkflows.Add(workflow);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Application Submitted", nameof(DemandApplication), applicationId);
            try
            {
                await _sms.SendAsync(application.Mobile, "ApplicationSubmitted", new Dictionary<string, string?> { ["ServiceName"] = application.ServiceDescription, ["ApplicationNumber"] = application.ApplicationNumber }, application.ApplicationNumber);
            }
            catch (Exception ex)
            {
                // Submission and the JE workflow have already been persisted.
                // A disabled/mock notification adapter must never turn a valid
                // applicant submission into a false HTTP 500 response.
                _logger.LogError(ex, "ApplicationSubmitted SMS event could not be recorded for demand application {ApplicationId}.", applicationId);
            }
        }
        return ToDto(await _db.DemandApplicationWorkflows.Include(x => x.DemandApplication).FirstAsync(x => x.Id == workflow.Id));
    }

    public Task<DemandWorkflowDto> VerifyJeAsync(int id, string actor, bool approve, string? reason) => Verify(id, actor, "JE", approve, reason, "OSPending", "JE Verified");
    public Task<DemandWorkflowDto> VerifyOsAsync(int id, string actor, bool approve, string? reason)
    {
        if (approve) throw new InvalidOperationException("पेमेंट विनंतीसाठी OS ने शुल्क रक्कम प्रविष्ट करणे आवश्यक आहे.");
        return Verify(id, actor, "OS", false, reason, "PaymentRequired", "OS Rejected");
    }

    public async Task<DemandWorkflowDto> CreatePaymentRequestAsync(int id, string actor, decimal payableAmount)
    {
        EnsureActor(actor, "OS");
        if (payableAmount <= 0m) throw new InvalidOperationException("कृपया वैध शुल्क रक्कम प्रविष्ट करा.");
        var workflow = await GetWorkflow(id);
        if (workflow.Stage != "OSPending") throw new InvalidOperationException("हा अर्ज OS पडताळणीसाठी प्रलंबित नाही.");

        workflow.PayableAmount = decimal.Round(payableAmount, 2, MidpointRounding.AwayFromZero);
        workflow.PaymentStatus = "PaymentRequired";
        workflow.Stage = "PaymentRequired";
        workflow.PaymentAccessToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        workflow.PaymentLink = BuildPaymentLink(workflow.DemandApplication.ApplicationNumber, workflow.PaymentAccessToken);
        workflow.UpdatedBy = actor;
        workflow.UpdatedAt = DateTime.UtcNow;
        workflow.DemandApplication.FeeAmount = workflow.PayableAmount;
        workflow.DemandApplication.PaymentStatus = "PaymentRequired";
        workflow.DemandApplication.Status = DemandApplicationStatus.FeePending;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Payment Request Sent", nameof(DemandApplication), id);
        try
        {
            await _sms.SendAsync(workflow.DemandApplication.Mobile, "PaymentRequired", new Dictionary<string, string?> { ["ApplicationNumber"] = workflow.DemandApplication.ApplicationNumber, ["Amount"] = workflow.PayableAmount.ToString("N2", CultureInfo.InvariantCulture), ["PaymentLink"] = workflow.PaymentLink }, workflow.DemandApplication.ApplicationNumber);
        }
        catch (Exception ex)
        {
            // The payment request is already durable. A disabled/mock notification
            // adapter must not turn it into a false workflow failure.
            _logger.LogError(ex, "PaymentRequired SMS event could not be recorded for demand application {ApplicationId}.", id);
        }
        return ToDto(workflow);
    }

    public async Task<DemandWorkflowDto> SubmitPaymentAsync(int id, PaymentConfirmationDto dto, Stream screenshot, string fileName, string contentType, string accessToken)
    {
        var workflow = await GetWorkflow(id);
        if (!FixedTimeEquals(workflow.PaymentAccessToken, accessToken)) throw new UnauthorizedAccessException("अवैध payment link.");
        if (workflow.Stage != "PaymentRequired" || workflow.PaymentStatus != "PaymentRequired") throw new InvalidOperationException("हा अर्ज सध्या पेमेंट पुष्टीकरणासाठी उपलब्ध नाही.");
        if (string.IsNullOrWhiteSpace(dto.Utr)) throw new InvalidOperationException("UTR / Transaction ID आवश्यक आहे.");
        if (!_storage.IsAllowedFile(fileName, screenshot.Length)) throw new InvalidOperationException("फक्त PDF/JPG/PNG/DOC/DOCX/XLSX फाईल (कमाल 10MB) परवानगी आहे.");
        if (await _db.DemandApplicationWorkflows.AnyAsync(x => x.Utr == dto.Utr && x.Id != workflow.Id)) throw new InvalidOperationException("हा UTR आधीच वापरला आहे.");
        var saved = await _storage.SaveFileAsync(screenshot, fileName, contentType, "demandpayments");
        workflow.Utr = dto.Utr.Trim();
        workflow.PaymentDate = dto.PaymentDate.Date;
        workflow.PaymentScreenshotPath = saved.filePath;
        workflow.PaymentScreenshotFileName = fileName;
        workflow.PaymentScreenshotSizeBytes = saved.size;
        workflow.PaymentSubmittedBy = "ApplicantPaymentLink";
        workflow.PaymentSubmittedAt = DateTime.UtcNow;
        workflow.PaymentStatus = "PaymentDone";
        workflow.Stage = "PaymentRequired";
        workflow.DemandApplication.PaymentStatus = "PaymentDone";
        workflow.DemandApplication.Status = DemandApplicationStatus.FeePending;
        workflow.UpdatedBy = "ApplicantPaymentLink";
        workflow.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Payment Submitted", nameof(DemandApplication), id);
        return ToDto(workflow);

        #pragma warning disable CS0162
        workflow.Utr = dto.Utr.Trim(); workflow.PaymentDate = dto.PaymentDate.Date; workflow.PaymentScreenshotPath = saved.filePath; workflow.PaymentScreenshotFileName = fileName; workflow.PaymentScreenshotSizeBytes = saved.size; workflow.PaymentSubmittedBy = "ApplicantPaymentLink"; workflow.PaymentSubmittedAt = DateTime.UtcNow; workflow.PaymentStatus = "PaymentVerificationPending"; workflow.Stage = "PaymentVerificationPending"; workflow.DemandApplication.PaymentStatus = "PaymentVerificationPending"; workflow.DemandApplication.Status = DemandApplicationStatus.FeePending; workflow.UpdatedBy = "ApplicantPaymentLink"; workflow.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Payment Submitted", nameof(DemandApplication), id);
        if (!workflow.OsPaymentNotificationSentAt.HasValue)
        {
            var osMobile = Environment.GetEnvironmentVariable("DEMAND_OS_MOBILE") ?? await _db.Users.Where(user => user.Role == UserRole.OS && user.IsActive && !user.IsDeleted).Select(user => user.Mobile).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(osMobile))
            {
                await _sms.SendAsync(osMobile, "PaymentSubmitted", new Dictionary<string, string?> { ["ApplicationNumber"] = workflow.DemandApplication.ApplicationNumber }, workflow.DemandApplication.ApplicationNumber);
                workflow.OsPaymentNotificationSentAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
        return ToDto(workflow);
    }

    public async Task<DemandWorkflowDto> VerifyPaymentAsync(int id, string actor, bool approve, string? reason)
    {
        EnsureActor(actor, "OS"); var workflow = await GetWorkflow(id); if (workflow.Stage != "PaymentVerificationPending") throw new InvalidOperationException("हा अर्ज पेमेंट पडताळणीसाठी प्रलंबित नाही.");
        if (!approve && string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("नकाराचे कारण आवश्यक आहे.");
        workflow.PaymentStatus = approve ? "PaymentVerified" : "PaymentRejected"; workflow.RejectionReason = approve ? null : reason; workflow.Stage = approve ? "AssistantCommissionerApprovalPending" : "PaymentRequired"; workflow.PaymentVerifiedBy = approve ? actor : null; workflow.PaymentVerifiedAt = approve ? DateTime.UtcNow : null; workflow.UpdatedBy = actor; workflow.UpdatedAt = DateTime.UtcNow;
        workflow.PaymentStatus = approve ? "PaymentDone" : "PaymentPending";
        workflow.Stage = "PaymentRequired";
        var app = workflow.DemandApplication; app.PaymentStatus = workflow.PaymentStatus; app.Status = DemandApplicationStatus.FeePending; await _db.SaveChangesAsync(); await _audit.LogAsync(approve ? "Payment Verified" : "Payment Rejected", nameof(DemandApplication), id);
        if (approve && workflow.Stage == "AssistantCommissionerApprovalPending")
        {
            await NotifyAssistantCommissionerAsync(app.ApplicationNumber);
        }
        return ToDto(workflow);
    }

    public async Task<DemandWorkflowDto> SetPaymentStatusAsync(int id, string actor, string paymentStatus)
    {
        EnsureActor(actor, "OS");
        if (paymentStatus is not ("PaymentPending" or "PaymentDone")) throw new InvalidOperationException("Invalid payment status.");
        var workflow = await GetWorkflow(id);
        if (workflow.Stage is not ("PaymentRequired" or "PaymentVerificationPending")) throw new InvalidOperationException("This application is not in a payment stage.");

        workflow.PaymentStatus = paymentStatus;
        workflow.Stage = "PaymentRequired";
        workflow.RejectionReason = null;
        workflow.PaymentVerifiedBy = paymentStatus == "PaymentDone" ? actor : null;
        workflow.PaymentVerifiedAt = paymentStatus == "PaymentDone" ? DateTime.UtcNow : null;
        workflow.UpdatedBy = actor;
        workflow.UpdatedAt = DateTime.UtcNow;
        workflow.DemandApplication.PaymentStatus = paymentStatus;
        workflow.DemandApplication.Status = DemandApplicationStatus.FeePending;
        await _db.SaveChangesAsync();
        await _audit.LogAsync($"Payment status set to {paymentStatus}", nameof(DemandApplication), id);
        return ToDto(workflow);
    }

    public async Task<DemandWorkflowDto> ForwardToAssistantCommissionerAsync(int id, string actor)
    {
        EnsureActor(actor, "OS");
        var workflow = await GetWorkflow(id);
        if (workflow.Stage != "PaymentRequired" || workflow.PaymentStatus != "PaymentDone") throw new InvalidOperationException("Payment must be completed before forwarding the application.");

        workflow.Stage = "AssistantCommissionerApprovalPending";
        workflow.UpdatedBy = actor;
        workflow.UpdatedAt = DateTime.UtcNow;
        workflow.DemandApplication.Status = DemandApplicationStatus.ApprovalPending;
        await _db.SaveChangesAsync();
        try
        {
            await _audit.LogAsync("Forwarded to Assistant Commissioner", nameof(DemandApplication), id);
        }
        catch (Exception ex)
        {
            // The transition is already committed; audit persistence must not
            // cause the officer UI to report a false workflow failure.
            _logger.LogError(ex, "Workflow audit event could not be recorded for forwarded demand application {ApplicationId}.", id);
        }
        await NotifyAssistantCommissionerAsync(workflow.DemandApplication.ApplicationNumber);
        return ToDto(workflow);
    }

    public async Task<DemandWorkflowDto> ApproveAsync(int id, string actor)
    {
        EnsureActor(actor, "AssistantCommissioner"); var workflow = await GetWorkflow(id); if (workflow.Stage != "AssistantCommissionerApprovalPending") throw new InvalidOperationException("हा अर्ज अंतिम मंजुरीसाठी प्रलंबित नाही.");
        workflow.Stage = "Approved"; workflow.ApprovedBy = actor; workflow.ApprovedAt = DateTime.UtcNow; workflow.CertificateFileName = $"Certificate-{workflow.DemandApplication.ApplicationNumber}.pdf"; workflow.CertificateGeneratedAt = DateTime.UtcNow; workflow.UpdatedBy = actor; workflow.UpdatedAt = DateTime.UtcNow; workflow.DemandApplication.Status = DemandApplicationStatus.Approved;
        var certificate = GeneratePdf(workflow, true);
        await using var certificateStream = new MemoryStream(certificate);
        var saved = await _storage.SaveFileAsync(certificateStream, workflow.CertificateFileName, "application/pdf", "demandcertificates");
        workflow.CertificateFilePath = saved.filePath;
        await _db.SaveChangesAsync(); await _audit.LogAsync("Certificate Generated", nameof(DemandApplication), id); await _audit.LogAsync("Final Approved", nameof(DemandApplication), id);
        try
        {
            await _sms.SendAsync(workflow.DemandApplication.Mobile, "ApplicationApproved", new Dictionary<string, string?> { ["ApplicationNumber"] = workflow.DemandApplication.ApplicationNumber }, workflow.DemandApplication.ApplicationNumber);
            await _sms.SendAsync(workflow.DemandApplication.Mobile, "CertificateAvailable", new Dictionary<string, string?> { ["ServiceName"] = workflow.DemandApplication.ServiceDescription, ["ApplicationNumber"] = workflow.DemandApplication.ApplicationNumber, ["CertificateLink"] = BuildCertificateLink(workflow.DemandApplication.ApplicationNumber, workflow.PaymentAccessToken) }, workflow.DemandApplication.ApplicationNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approval notification event could not be recorded for demand application {ApplicationId}.", id);
        }
        return ToDto(workflow);
    }

    public async Task<DemandWorkflowDto> RejectFinalAsync(int id, string actor, string? reason)
    {
        EnsureActor(actor, "AssistantCommissioner");
        if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("नकाराचे कारण आवश्यक आहे.");
        var workflow = await GetWorkflow(id);
        if (workflow.Stage != "AssistantCommissionerApprovalPending") throw new InvalidOperationException("हा अर्ज अंतिम मंजुरीसाठी प्रलंबित नाही.");
        workflow.Stage = "Rejected";
        workflow.RejectionReason = reason.Trim();
        workflow.UpdatedBy = actor;
        workflow.UpdatedAt = DateTime.UtcNow;
        workflow.DemandApplication.Status = DemandApplicationStatus.Rejected;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Assistant Commissioner Rejected", nameof(DemandApplication), id);
        return ToDto(workflow);
    }

    public async Task<(byte[] content, string fileName)> GenerateApplicationPdfAsync(int applicationId)
    {
        var workflow = await GetWorkflow(applicationId);
        return (GenerateApplicationPdf(workflow), $"Demand-Application-{workflow.DemandApplication.ApplicationNumber}.pdf");
    }

    private async Task<DemandWorkflowDto> Verify(int id, string actor, string role, bool approve, string? reason, string nextStage, string action)
    {
        EnsureActor(actor, role); var workflow = await GetWorkflow(id); var expected = role == "JE" ? "JEPending" : "OSPending"; if (workflow.Stage != expected) throw new InvalidOperationException("हा अर्ज या अधिकाऱ्याच्या पडताळणीसाठी प्रलंबित नाही."); if (!approve && string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("नकाराचे कारण आवश्यक आहे.");
        workflow.RejectionReason = approve ? null : reason;
        workflow.Stage = approve ? nextStage : "Rejected";
        workflow.UpdatedBy = actor; workflow.UpdatedAt = DateTime.UtcNow;
        workflow.DemandApplication.Status = !approve ? DemandApplicationStatus.Rejected : DemandApplicationStatus.UnderScrutiny;
        await _db.SaveChangesAsync(); await _audit.LogAsync(approve ? action : $"{role} Rejected", nameof(DemandApplication), id);
        return ToDto(workflow);
    }

    private async Task<DemandApplicationWorkflow> GetWorkflow(int id) => await _db.DemandApplicationWorkflows.Include(x => x.DemandApplication).ThenInclude(x => x.Documents).FirstOrDefaultAsync(x => x.DemandApplicationId == id) ?? throw new InvalidOperationException("वर्कफ्लो नोंद सापडली नाही.");
    private async Task<DemandWorkflowDto?> FindDto(int id) { var x = await _db.DemandApplicationWorkflows.Include(x => x.DemandApplication).FirstOrDefaultAsync(x => x.DemandApplicationId == id); return x is null ? null : ToDto(x); }
    private async Task<DemandApplicationWorkflow?> PublicWorkflow(string number, string token) { var workflow = await _db.DemandApplicationWorkflows.Include(x => x.DemandApplication).FirstOrDefaultAsync(x => x.DemandApplication.ApplicationNumber == number && !x.DemandApplication.IsDeleted); return workflow is not null && FixedTimeEquals(workflow.PaymentAccessToken, token) ? workflow : null; }
    private static string BuildPaymentLink(string number, string token) => $"{Environment.GetEnvironmentVariable("APP_BASE_URL")?.TrimEnd('/') ?? "http://localhost:3000"}/application-status?applicationNumber={Uri.EscapeDataString(number)}&token={Uri.EscapeDataString(token)}";
    private static string? BuildCertificateLink(string number, string? token) => string.IsNullOrWhiteSpace(token) ? null : $"{Environment.GetEnvironmentVariable("APP_BASE_URL")?.TrimEnd('/') ?? "http://localhost:3000"}/api/demand-workflow/payment/{Uri.EscapeDataString(number)}/certificate-pdf?token={Uri.EscapeDataString(token)}";
    private async Task NotifyAssistantCommissionerAsync(string applicationNumber)
    {
        var acMobile = Environment.GetEnvironmentVariable("DEMAND_AC_MOBILE") ?? await _db.Users.Where(user => user.Role == UserRole.AssistantCommissioner && user.IsActive && !user.IsDeleted).Select(user => user.Mobile).FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(acMobile)) return;
        try
        {
            await _sms.SendAsync(acMobile, "AssistantCommissionerNotification", new Dictionary<string, string?> { ["ApplicationNumber"] = applicationNumber }, applicationNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Assistant Commissioner notification event could not be recorded for application {ApplicationNumber}.", applicationNumber);
        }
    }
    private static bool FixedTimeEquals(string? expected, string? supplied) => !string.IsNullOrWhiteSpace(expected) && !string.IsNullOrWhiteSpace(supplied) && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(expected), System.Text.Encoding.UTF8.GetBytes(supplied));
    private static byte[] GeneratePdf(DemandApplicationWorkflow workflow, bool certificate)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var app = workflow.DemandApplication;
        return QuestPDF.Fluent.Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(32);
            page.Header().AlignCenter().Column(c => { c.Item().Text("सोलापूर महानगरपालिका").FontSize(18).Bold(); c.Item().Text("भूमी व मालमत्ता व्यवस्थापन प्रणाली"); c.Item().PaddingTop(8).Text(certificate ? "जागा मागणी अर्ज मंजुरी प्रमाणपत्र" : "मागणी अर्ज").FontSize(16).Bold(); });
            page.Content().PaddingVertical(18).Column(c =>
            {
                void Row(string label, string? value) => c.Item().Row(r => { r.RelativeItem(2).Text(label).Bold(); r.RelativeItem(3).Text(value ?? "-"); });
                Row("अर्ज क्रमांक", app.ApplicationNumber); Row("अर्जदाराचे नाव", app.ApplicantName); Row("मोबाईल क्रमांक", app.Mobile); Row("सेवा प्रकार", app.ServiceType.ToString());
                if (app.BusinessType.HasValue) Row("व्यवसाय प्रकार", app.BusinessType == DemandBusinessType.Other ? app.OtherBusinessType : app.BusinessType.ToString());
                Row("पत्ता", app.PermanentAddress); Row("तालुका", app.Taluka); Row("प्रभाग", app.Prabhag); Row("ठिकाण", app.Location); Row("मंजूर क्षेत्रफळ", app.AreaSqFt?.ToString("N2") + " sq.ft."); Row("कालावधी", $"{app.StartDate:dd-MM-yyyy} ते {app.EndDate:dd-MM-yyyy} ({app.RequiredDuration} दिवस)"); Row("पेमेंट रक्कम", $"₹{workflow.PayableAmount:N2}"); Row("UTR संदर्भ", workflow.Utr);
                if (certificate) { Row("मंजुरी दिनांक", workflow.ApprovedAt?.ToString("dd-MM-yyyy")); Row("सहाय्यक आयुक्त", workflow.ApprovedBy); c.Item().PaddingTop(24).Text("सदर जागा मागणी अर्ज मंजूर करण्यात आला आहे.").Bold(); }
            });
            page.Footer().AlignCenter().Text($"तयार दिनांक: {DateTime.UtcNow:dd-MM-yyyy}");
        })).GeneratePdf();
    }

    // Kept separate from the certificate renderer: this is the persisted,
    // server-side applicant form, not an html2canvas browser snapshot.
    private static byte[] GenerateApplicationPdf(DemandApplicationWorkflow workflow)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var app = workflow.DemandApplication;
        return QuestPDF.Fluent.Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(30);
            page.DefaultTextStyle(style => style.FontSize(9).FontFamily("Nirmala UI"));
            page.Header().AlignCenter().Column(column =>
            {
                column.Item().Text("सोलापूर महानगरपालिका").FontSize(17).Bold();
                column.Item().Text("भूमी व मालमत्ता व्यवस्थापन विभाग").FontSize(11);
                column.Item().PaddingTop(5).Text("मागणी अर्ज").FontSize(15).Bold();
                column.Item().PaddingTop(7).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
            });
            page.Content().PaddingVertical(12).Column(column =>
            {
                void Section(string title) => column.Item().PaddingTop(7).Background(Colors.Blue.Lighten5).Padding(6).Text(title).FontSize(11).Bold().FontColor(Colors.Blue.Darken3);
                void Row(string label, string? value) => column.Item().Row(row => { row.RelativeItem(2).PaddingVertical(2).Text(label).Bold(); row.RelativeItem(3).PaddingVertical(2).Text(string.IsNullOrWhiteSpace(value) ? "-" : value); });
                string YesNo(bool value) => value ? "होय" : "नाही";
                string Date(DateTime? value) => value?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) ?? "-";
                string BusinessType() => app.BusinessType == DemandBusinessType.Other ? app.OtherBusinessType ?? "इतर" : app.BusinessType switch
                {
                    DemandBusinessType.FirecrackerStall => "फटाके स्टॉल", DemandBusinessType.GanpatiIdolStall => "गणपती मूर्ती स्टॉल", DemandBusinessType.RangpanchamiStall => "रंगपंचमी स्टॉल", DemandBusinessType.RakshabandhanStall => "रक्षाबंधन स्टॉल", DemandBusinessType.DiwaliFaralStall => "दिवाळी फराळ स्टॉल", _ => "-"
                };

                Row("अर्ज क्रमांक", app.ApplicationNumber); Row("अर्ज सादर दिनांक", Date(app.SubmittedAt ?? app.CreatedAt)); Row("स्थिती", app.Status.ToString());
                Section("१. अर्जदाराची माहिती");
                Row("पूर्ण नाव", app.ApplicantName); Row("मोबाईल", app.Mobile); Row("ई-मेल", app.Email); Row("पत्ता", app.PermanentAddress);
                Row("राज्य / जिल्हा / शहर", $"{app.State} / {app.District} / {app.City}"); Row("तालुका / प्रभाग / PIN", $"{app.Taluka} / {app.Prabhag} / {app.PinCode}");
                Section("२. मागणीची माहिती");
                Row("मालमत्ता / जागेचा प्रकार", app.ServiceType.ToString()); Row("व्यवसायाचा प्रकार", BusinessType()); Row("सेवा/विक्रीचा प्रकार", app.ServiceDescription); Row("स्टॉल/जागेची आवश्यकता (क्षेत्रफळामध्ये)", app.SpaceRequirement); Row("वापराचा उद्देश", app.ServiceDescription); Row("शेरा", app.OtherInformation);
                Row("कालावधी", $"{Date(app.StartDate)} ते {Date(app.EndDate)} ({app.RequiredDuration} दिवस)"); Row("वीज सुविधा", YesNo(app.ElectricityRequired)); Row("पाणी सुविधा", YesNo(app.WaterRequired)); Row("इतर आवश्यक सुविधा", app.OtherFacilities); Row("कचरा व्यवस्थापन / संबंधित आवश्यकता", app.WasteManagement);
                Section("३. संलग्न कागदपत्रे");
                var documents = app.Documents.Where(document => !document.IsDeleted).ToList();
                if (documents.Count == 0) Row("कागदपत्रांची माहिती", "-"); else foreach (var document in documents) Row(document.DocumentType, document.FileName);
                Section("घोषणा"); column.Item().PaddingTop(3).Text($"मी दिलेली माहिती खरी असून नियम व अटी मान्य आहेत. घोषणा स्वीकारली: {YesNo(app.DeclarationAccepted)}");
                column.Item().PaddingTop(20).Row(row => { row.RelativeItem().Text("अर्जदाराची सही: ____________________"); row.RelativeItem().AlignRight().Text("दिनांक: ____________________"); });
            });
        })).GeneratePdf();
    }
    private string RoleFor(string actor) => _currentUser.Role switch { "JE" => "JE", "OS" => "OS", "AssistantCommissioner" or "Admin" => "AssistantCommissioner", _ => "Applicant" };
    private void EnsureActor(string actor, params string[] roles) { if (!roles.Contains(RoleFor(actor), StringComparer.OrdinalIgnoreCase)) throw new UnauthorizedAccessException("या कृतीसाठी आपल्याला अधिकार नाहीत."); }
    private static DemandWorkflowDto ToDto(DemandApplicationWorkflow x) => new() { Id = x.Id, DemandApplicationId = x.DemandApplicationId, ApplicationNumber = x.DemandApplication.ApplicationNumber, ApplicantName = x.DemandApplication.ApplicantName, Mobile = x.DemandApplication.Mobile, ServiceDescription = x.DemandApplication.ServiceDescription, SpaceRequirement = x.DemandApplication.SpaceRequirement, SubmittedAt = x.DemandApplication.SubmittedAt, ApplicationStatus = x.DemandApplication.Status.ToString(), Stage = x.Stage, PayableAmount = x.PayableAmount, PaymentStatus = x.PaymentStatus, PaymentLink = x.PaymentLink, Utr = x.Utr, PaymentDate = x.PaymentDate, PaymentScreenshotFileName = x.PaymentScreenshotFileName, PaymentScreenshotPath = x.PaymentScreenshotPath, RejectionReason = x.RejectionReason, CertificateFileName = x.CertificateFileName, CertificateFilePath = x.CertificateFilePath };
}
