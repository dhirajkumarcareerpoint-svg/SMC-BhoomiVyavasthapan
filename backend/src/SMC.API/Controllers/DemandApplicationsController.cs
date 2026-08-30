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
    [HttpDelete("{id:int}")][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> Delete(int id)=>Ok(ApiResponse<object>.Ok(new {Result=await _service.DeleteAsync(id,_user.UserName??"System")}));
    [HttpPost("{id:int}/submit")][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> Submit(int id){var result=await _service.SubmitAsync(id,_user.UserName??"System")??throw new InvalidOperationException("अर्ज सापडला नाही.");await _workflow.EnsureAsync(id,_user.UserName??"System");return Ok(ApiResponse<DemandApplicationDto>.Ok(result));}
    [HttpPost("{id:int}/documents")][Authorize(Policy="AdminOrOfficer")][RequestSizeLimit(10_000_000)] public async Task<IActionResult> Upload(int id,[FromForm]string documentType,IFormFile file){if(file is null)return BadRequest(ApiResponse<object>.Fail("कृपया फाईल निवडा."));await using var stream=file.OpenReadStream();return Ok(ApiResponse<DemandApplicationDocumentDto>.Ok(await _service.AddDocumentAsync(id,documentType,stream,file.FileName,file.ContentType,_user.UserName??"System")));}
    [HttpDelete("{id:int}/documents/{documentId:int}")][Authorize(Policy="AdminOrOfficer")] public async Task<IActionResult> DeleteDocument(int id,int documentId)=>Ok(ApiResponse<object>.Ok(new {Result=await _service.DeleteDocumentAsync(id,documentId,_user.UserName??"System")}));
    [HttpGet("documents/{documentId:int}/download")] public async Task<IActionResult> Download(int documentId){var d=await _service.GetDocumentAsync(documentId);if(d is null)return NotFound();var root=_config["FileStorage:RootPath"]??Path.Combine(AppContext.BaseDirectory,"UploadedFiles");var path=Path.Combine(root,d.FilePath);if(!System.IO.File.Exists(path))return NotFound();return File(await System.IO.File.ReadAllBytesAsync(path),d.ContentType,d.FileName);}

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
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadPublic(int id, [FromForm]string documentType, IFormFile file, [FromHeader(Name = "X-Demand-Application-Token")] string? accessToken)
    {
        if (file is null) return BadRequest(ApiResponse<object>.Fail("कृपया फाईल निवडा."));
        await using var stream = file.OpenReadStream();
        var result = await _service.AddPublicDocumentAsync(id, documentType, stream, file.FileName, file.ContentType, accessToken);
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
    private static async Task<IActionResult> PublicResult(Task<DemandApplicationDto?> task)
    {
        var result = await task;
        return result is null ? new NotFoundResult() : new OkObjectResult(ApiResponse<DemandApplicationDto>.Ok(result));
    }
}
