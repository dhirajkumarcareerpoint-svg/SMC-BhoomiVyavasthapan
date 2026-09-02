using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

[ApiController]
[Route("api/demand-workflow")]
[Authorize(Policy = "AllStaff")]
public class DemandWorkflowController : ControllerBase
{
    private readonly IDemandWorkflowService _service;
    private readonly ICurrentUserService _user;
    private readonly IConfiguration _configuration;

    public DemandWorkflowController(IDemandWorkflowService service, ICurrentUserService user, IConfiguration configuration)
    { _service = service; _user = user; _configuration = configuration; }

    [HttpGet("{applicationId:int}")]
    public async Task<IActionResult> Get(int applicationId) => Ok(ApiResponse<DemandWorkflowDto?>.Ok(await _service.GetAsync(applicationId)));

    [AllowAnonymous]
    [HttpGet("payment/{applicationNumber}")]
    public async Task<IActionResult> PublicPayment(string applicationNumber, [FromQuery] string token)
    {
        var payment = await _service.GetPublicPaymentAsync(applicationNumber, token);
        return payment is null ? NotFound(ApiResponse<object>.Fail("अवैध payment link.")) : Ok(ApiResponse<PublicPaymentDto>.Ok(payment));
    }

    [AllowAnonymous]
    [HttpGet("public-status/{applicationNumber}")]
    public async Task<IActionResult> PublicStatus(string applicationNumber, [FromQuery(Name = "token")] string? paymentAccessToken, [FromQuery] string? requestToken)
    {
        var status = await _service.GetPublicStatusAsync(applicationNumber, paymentAccessToken, requestToken);
        return status is null ? NotFound(ApiResponse<object>.Fail("दिलेल्या अर्ज क्रमांकाची माहिती उपलब्ध नाही.")) : Ok(ApiResponse<PublicDemandApplicationStatusDto>.Ok(status));
    }

    [AllowAnonymous]
    [HttpGet("payment/{applicationNumber}/application-pdf")]
    public async Task<IActionResult> PublicApplicationPdf(string applicationNumber, [FromQuery] string token)
    {
        var payment = await _service.GetPublicPaymentAsync(applicationNumber, token);
        if (payment is null) return NotFound();
        var pdf = await _service.GenerateApplicationPdfAsync(payment.DemandApplicationId);
        return File(pdf.content, "application/pdf", pdf.fileName);
    }

    [AllowAnonymous]
    [HttpGet("payment/{applicationNumber}/certificate-pdf")]
    public async Task<IActionResult> PublicCertificatePdf(string applicationNumber, [FromQuery] string token)
    {
        var payment = await _service.GetPublicPaymentAsync(applicationNumber, token);
        if (payment?.CertificateFileName is null || payment.CertificateFilePath is null) return NotFound();
        var root = _configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedFiles");
        var path = Path.Combine(root, payment.CertificateFilePath);
        if (!System.IO.File.Exists(path)) return NotFound();
        return File(await System.IO.File.ReadAllBytesAsync(path), "application/pdf", payment.CertificateFileName);
    }

    [HttpGet("queue")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> Queue() => Ok(ApiResponse<List<DemandWorkflowDto>>.Ok(await _service.QueueAsync(_user.UserName ?? "System")));

    [HttpGet("processed-history")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> ProcessedHistory() => Ok(ApiResponse<List<ProcessedDemandWorkflowDto>>.Ok(await _service.ProcessedHistoryAsync(_user.UserName ?? "System")));

    [HttpPost("{applicationId:int}/ensure")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> Ensure(int applicationId) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.EnsureAsync(applicationId, _user.UserName ?? "System")));

    [HttpPost("{applicationId:int}/je")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> Je(int applicationId, [FromBody] WorkflowDecisionDto dto) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.VerifyJeAsync(applicationId, _user.UserName ?? "System", dto.Approve, dto.Reason)));

    [HttpPost("{applicationId:int}/os")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> Os(int applicationId, [FromBody] WorkflowDecisionDto dto) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.VerifyOsAsync(applicationId, _user.UserName ?? "System", dto.Approve, dto.Reason)));

    [HttpPost("{applicationId:int}/payment-request")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> PaymentRequest(int applicationId) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.CreatePaymentRequestAsync(applicationId, _user.UserName ?? "System")));

    [HttpPost("{applicationId:int}/payment")]
    [AllowAnonymous]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Payment(int applicationId, [FromForm] string utr, [FromForm] DateTime paymentDate, [FromForm] string token, IFormFile screenshot)
    {
        if (screenshot is null) return BadRequest(ApiResponse<object>.Fail("कृपया पेमेंट पावती निवडा."));
        await using var stream = screenshot.OpenReadStream();
        var result = await _service.SubmitPaymentAsync(applicationId, new PaymentConfirmationDto { Utr = utr, PaymentDate = paymentDate }, stream, screenshot.FileName, screenshot.ContentType, token);
        return Ok(ApiResponse<DemandWorkflowDto>.Ok(result));
    }

    [HttpPost("{applicationId:int}/payment/verify")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> VerifyPayment(int applicationId, [FromBody] WorkflowDecisionDto dto) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.VerifyPaymentAsync(applicationId, _user.UserName ?? "System", dto.Approve, dto.Reason)));

    [HttpPost("{applicationId:int}/payment-status")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> SetPaymentStatus(int applicationId, [FromBody] PaymentStatusDto dto) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.SetPaymentStatusAsync(applicationId, _user.UserName ?? "System", dto.Status)));

    [HttpPost("{applicationId:int}/forward-to-assistant-commissioner")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> ForwardToAssistantCommissioner(int applicationId) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.ForwardToAssistantCommissionerAsync(applicationId, _user.UserName ?? "System")));

    [HttpGet("{applicationId:int}/payment/receipt")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> PaymentReceipt(int applicationId)
    {
        var workflow = await _service.GetAsync(applicationId);
        if (workflow?.PaymentScreenshotPath is null || workflow.PaymentScreenshotFileName is null) return NotFound();
        var root = _configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedFiles");
        var path = Path.Combine(root, workflow.PaymentScreenshotPath);
        if (!System.IO.File.Exists(path)) return NotFound();
        return File(await System.IO.File.ReadAllBytesAsync(path), "application/octet-stream", workflow.PaymentScreenshotFileName);
    }

    [HttpGet("{applicationId:int}/application-pdf")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> ApplicationPdf(int applicationId)
    {
        var pdf = await _service.GenerateApplicationPdfAsync(applicationId);
        return File(pdf.content, "application/pdf", pdf.fileName);
    }

    [HttpGet("{applicationId:int}/certificate-pdf")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> CertificatePdf(int applicationId)
    {
        var workflow = await _service.GetAsync(applicationId);
        if (workflow?.CertificateFileName is null || workflow.CertificateFilePath is null) return NotFound();
        var root = _configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedFiles");
        var path = Path.Combine(root, workflow.CertificateFilePath);
        if (!System.IO.File.Exists(path)) return NotFound();
        return File(await System.IO.File.ReadAllBytesAsync(path), "application/pdf", workflow.CertificateFileName);
    }

    [HttpPost("{applicationId:int}/approve")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> Approve(int applicationId) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.ApproveAsync(applicationId, _user.UserName ?? "System")));

    [HttpPost("{applicationId:int}/reject")]
    [Authorize(Policy = "DemandOfficer")]
    public async Task<IActionResult> Reject(int applicationId, [FromBody] WorkflowDecisionDto dto) => Ok(ApiResponse<DemandWorkflowDto>.Ok(await _service.RejectFinalAsync(applicationId, _user.UserName ?? "System", dto.Reason)));
}

public class WorkflowDecisionDto
{
    public bool Approve { get; set; }
    public string? Reason { get; set; }
}

public class PaymentStatusDto
{
    public string Status { get; set; } = string.Empty;
}
