using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>सर्व sections साठी सामायिक Document upload/view/delete.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public DocumentsController(IDocumentService service, ICurrentUserService currentUser, IWebHostEnvironment env, IConfiguration config)
    {
        _service = service;
        _currentUser = currentUser;
        _env = env;
        _config = config;
    }

    /// <summary>entityType: Property, Lease, RecoveryCase, Scheme, Allocation, Calculation</summary>
    [HttpGet]
    public async Task<IActionResult> GetByEntity([FromQuery] string entityType, [FromQuery] int entityId) =>
        Ok(ApiResponse<List<DocumentDto>>.Ok(await _service.GetByEntityAsync(entityType, entityId)));

    [HttpPost("upload")]
    [Authorize(Policy = "AdminOrOfficer")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload([FromForm] string entityType, [FromForm] int entityId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("कृपया फाईल निवडा."));

        await using var stream = file.OpenReadStream();
        var doc = await _service.UploadAsync(entityType, entityId, stream, file.FileName, file.ContentType, _currentUser.UserName ?? "System");
        return Ok(ApiResponse<DocumentDto>.Ok(doc, "दस्तऐवज यशस्वीरित्या अपलोड झाला."));
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _service.GetEntityAsync(id);
        if (doc is null) return NotFound(ApiResponse<object>.Fail("दस्तऐवज सापडला नाही."));

        var rootPath = _config["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "UploadedFiles");
        var fullPath = Path.Combine(rootPath, doc.FilePath);
        if (!System.IO.File.Exists(fullPath)) return NotFound(ApiResponse<object>.Fail("फाईल डिस्कवर सापडली नाही."));

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(bytes, string.IsNullOrEmpty(doc.ContentType) ? "application/octet-stream" : doc.ContentType, doc.FileName);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "दस्तऐवज हटवला.")) : NotFound(ApiResponse<object>.Fail("दस्तऐवज सापडला नाही."));
    }
}
