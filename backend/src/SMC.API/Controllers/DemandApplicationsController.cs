using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

[ApiController]
[Route("api/demand-applications")]
[Authorize(Policy="AllStaff")]
public class DemandApplicationsController : ControllerBase
{
    private readonly IDemandApplicationService _service; private readonly IDemandWorkflowService _workflow; private readonly ICurrentUserService _user; private readonly IConfiguration _config;
    public DemandApplicationsController(IDemandApplicationService service, IDemandWorkflowService workflow, ICurrentUserService user, IConfiguration config){_service=service;_workflow=workflow;_user=user;_config=config;}
    [HttpGet] public async Task<IActionResult> GetAll()=>Ok(ApiResponse<List<DemandApplicationDto>>.Ok(await _service.GetAllAsync()));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id){var x=await _service.GetByIdAsync(id);return x is null?NotFound():Ok(ApiResponse<DemandApplicationDto>.Ok(x));}
    [HttpPost][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> Create(CreateDemandApplicationDto dto)=>Ok(ApiResponse<DemandApplicationDto>.Ok(await _service.CreateAsync(dto,_user.UserName??"System")));
    [HttpPut("{id:int}")][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> Update(int id,UpdateDemandApplicationDto dto)=>Ok(ApiResponse<object>.Ok(new {Result=await _service.UpdateAsync(id,dto,_user.UserName??"System")}));
    [HttpDelete("{id:int}")][Authorize(Policy="DemandOfficer")] public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id, _user.UserName ?? "System");
        return deleted
            ? Ok(ApiResponse<object>.Ok(new { Result = true }, "अर्ज यशस्वीरीत्या हटवला आहे."))
            : NotFound(ApiResponse<object>.Fail("अर्ज सापडला नाही किंवा आधीच हटवला आहे."));
    }
    [HttpPost("{id:int}/submit")][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> Submit(int id){var result=await _service.SubmitAsync(id,_user.UserName??"System")??throw new InvalidOperationException("अर्ज सापडला नाही.");await _workflow.EnsureAsync(id,_user.UserName??"System");return Ok(ApiResponse<DemandApplicationDto>.Ok(result));}
    [HttpPost("{id:int}/documents")][Authorize(Policy="AdminOrOfficer")][RequestSizeLimit(22_000_000)] public async Task<IActionResult> Upload(int id,[FromForm]string documentType,IFormFile file){if(!IsValidDemandDocument(file,out var error))return BadRequest(ApiResponse<object>.Fail(error));await using var stream=file.OpenReadStream();return Ok(ApiResponse<DemandApplicationDocumentDto>.Ok(await _service.AddDocumentAsync(id,documentType,stream,file.FileName,file.ContentType,_user.UserName??"System")));}
    [HttpDelete("{id:int}/documents/{documentId:int}")][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> DeleteDocument(int id,int documentId)=>Ok(ApiResponse<object>.Ok(new {Result=await _service.DeleteDocumentAsync(id,documentId,_user.UserName??"System")}));
    [HttpGet("documents/{documentId:int}/download")][Authorize(Policy="DemandOfficer")] public async Task<IActionResult> Download(int documentId){var d=await _service.GetDocumentAsync(documentId);if(d is null)return NotFound();var root=_config["FileStorage:RootPath"]??Path.Combine(AppContext.BaseDirectory,"UploadedFiles");var path=Path.Combine(root,d.FilePath);if(!System.IO.File.Exists(path))return NotFound();return File(await System.IO.File.ReadAllBytesAsync(path),d.ContentType,d.FileName);}
    [HttpPost("{id:int}/documents/{documentId:int}/verification")][Authorize(Roles="OS")] public async Task<IActionResult> VerifyDocument(int id,int documentId,DocumentVerificationDto dto){var document=await _service.SetDocumentVerificationAsync(id,documentId,dto.Status,_user.UserName??"System",dto.Remark);if(document.RequestToken is not null){var application=await _service.GetByIdAsync(id);var publicBase=Environment.GetEnvironmentVariable("APP_BASE_URL")?.TrimEnd('/') ?? "http://localhost:3000";document.SecureRequestUrl=$"{publicBase}/application-status?applicationNumber={Uri.EscapeDataString(application!.ApplicationNumber)}&requestToken={Uri.EscapeDataString(document.RequestToken)}";document.RequestToken=null;}return Ok(ApiResponse<DemandApplicationDocumentDto>.Ok(document));}
    [HttpPost("{id:int}/site-photo")]
    [Authorize(Roles = "OS")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadSitePhoto(int id, IFormFile file)
    {
        if (!IsValidSitePhoto(file, out var error)) return BadRequest(ApiResponse<object>.Fail(error));
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<DemandApplicationDocumentDto>.Ok(await _service.AddSiteInspectionPhotoAsync(id, stream, file.FileName, file.ContentType, _user.UserName ?? "System")));
    }

    // Public applicant lifecycle. Every operation after creation requires the
    // per-application capability token in a request header; staff endpoints above
    // intentionally remain protected by the controller's AllStaff policy.
    [AllowAnonymous]
    [HttpPost("public")]
    public async Task<IActionResult> CreatePublic(CreateDemandApplicationDto dto) => Ok(ApiResponse<PublicDemandApplicationSessionDto>.Ok(await _service.CreatePublicAsync(dto)));
    [AllowAnonymous]
    [HttpGet("public/{id:int}")]
    public async Task<IActionResult> GetPublic(int id, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken) => await PublicResult(_service.GetPublicAsync(id, accessToken));
    [AllowAnonymous]
    [HttpGet("public/{id:int}/application-pdf")]
    public async Task<IActionResult> DownloadPublicApplicationPdf(int id, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken)
    {
        var application = await _service.GetPublicAsync(id, accessToken);
        if (application is null) return NotFound();
        var pdf = await _workflow.GenerateApplicationPdfAsync(id);
        return File(pdf.content, "application/pdf", pdf.fileName);
    }
    [AllowAnonymous]
    [HttpPut("public/{id:int}")]
    public async Task<IActionResult> UpdatePublic(int id, UpdateDemandApplicationDto dto, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken) => await PublicResult(_service.UpdatePublicAsync(id, dto, accessToken));
    [AllowAnonymous]
    [HttpPost("public/{id:int}/submit")]
    public async Task<IActionResult> SubmitPublic(int id, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken)
    {
        var result = await _service.SubmitPublicAsync(id, accessToken);
        if (result is null) return NotFound();
        await _workflow.EnsureAsync(id, "Applicant");
        return Ok(ApiResponse<DemandApplicationDto>.Ok(result));
    }
    [AllowAnonymous]
    [HttpPost("public/{id:int}/documents")]
    [RequestSizeLimit(22_000_000)]
    public async Task<IActionResult> UploadPublic(int id, [FromForm]string documentType, IFormFile file, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken)
    {
        if (!IsValidDemandDocument(file, out var error)) return BadRequest(ApiResponse<object>.Fail(error));
        await using var stream = file.OpenReadStream();
        var result = await _service.AddPublicDocumentAsync(id, documentType, stream, file.FileName, file.ContentType, accessToken);
        return result is null ? NotFound() : Ok(ApiResponse<DemandApplicationDocumentDto>.Ok(result));
    }
    [AllowAnonymous]
    [HttpPost("public/{id:int}/documents/{documentId:int}/resubmit")]
    [RequestSizeLimit(22_000_000)]
    public async Task<IActionResult> ResubmitPublicDocument(int id, int documentId, IFormFile file, [FromHeader(Name = "X-Demand-Document-Request-Token")] string? requestToken)
    {
        if (!IsValidDemandDocument(file, out var error)) return BadRequest(ApiResponse<object>.Fail(error));
        await using var stream=file.OpenReadStream(); var result=await _service.ResubmitPublicDocumentAsync(id,documentId,stream,file.FileName,file.ContentType,requestToken);
        return result is null ? NotFound() : Ok(ApiResponse<DemandApplicationDocumentDto>.Ok(result));
    }
    [AllowAnonymous]
    [HttpDelete("public/{id:int}/documents/{documentId:int}")]
    public async Task<IActionResult> DeletePublicDocument(int id, int documentId, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken)
        => await _service.DeletePublicDocumentAsync(id, documentId, accessToken) ? Ok(ApiResponse<object>.Ok(new { Result = true })) : NotFound();
    [AllowAnonymous]
    [HttpGet("public/{id:int}/documents/{documentId:int}/download")]
    public async Task<IActionResult> DownloadPublicDocument(int id, int documentId, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken)
    {
        var d = await _service.GetPublicDocumentAsync(id, documentId, accessToken);
        if (d is null) return NotFound();
        var root = _config["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedFiles");
        var path = Path.Combine(root, d.FilePath);
        return !System.IO.File.Exists(path) ? NotFound() : File(await System.IO.File.ReadAllBytesAsync(path), d.ContentType, d.FileName);
    }
    private static bool IsValidDemandDocument(IFormFile? file, out string error)
    {
        if (file is null || file.Length == 0) { error = "कृपया फाईल निवडा."; return false; }
        if (file.Length > 5 * 1024 * 1024) { error = "कागदपत्राचा आकार 5 MB पेक्षा जास्त असू शकत नाही."; return false; }
        var extension = Path.GetExtension(file.FileName);
        var isPdf = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) && string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
        var isDocx = string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase) && string.Equals(file.ContentType, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);
        if (!isPdf && !isDocx) { error = "फक्त PDF किंवा DOCX स्वरूपातील कागदपत्र अपलोड करा."; return false; }
        if (isPdf)
        {
            using var stream = file.OpenReadStream(); var header = new byte[5];
            if (stream.Read(header, 0, header.Length) != 5 || System.Text.Encoding.ASCII.GetString(header) != "%PDF-") { error = "फक्त वैध PDF स्वरूपातील कागदपत्र अपलोड करा."; return false; }
        }
        error = string.Empty; return true;
    }
    private static bool IsValidSitePhoto(IFormFile? file, out string error)
    {
        if (file is null || file.Length == 0) { error = "कृपया प्रतिमा निवडा."; return false; }
        if (file.Length > 10 * 1024 * 1024) { error = "प्रतिमेचा आकार 10 MB पेक्षा जास्त असू शकत नाही."; return false; }
        var isJpeg = string.Equals(file.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) && (Path.GetExtension(file.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(file.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase));
        var isPng = string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase) && Path.GetExtension(file.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase);
        if (!isJpeg && !isPng) { error = "फक्त JPG किंवा PNG प्रतिमा अपलोड करा."; return false; }
        using var stream = file.OpenReadStream(); var header = new byte[8]; var read = stream.Read(header, 0, header.Length);
        var jpegSignature = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var pngSignature = read == 8 && header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        if (!jpegSignature && !pngSignature) { error = "फक्त वैध JPG किंवा PNG प्रतिमा अपलोड करा."; return false; }
        error = string.Empty; return true;
    }
    private static async Task<IActionResult> PublicResult(Task<DemandApplicationDto?> task)
    {
        var result = await task;
        return result is null ? new NotFoundResult() : new OkObjectResult(ApiResponse<DemandApplicationDto>.Ok(result));
    }
}
