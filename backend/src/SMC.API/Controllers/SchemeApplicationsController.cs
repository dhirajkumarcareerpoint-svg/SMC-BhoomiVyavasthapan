using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>विविध उपक्रम (Tab 4) - अभय योजना, दंडमाफी, सवलत, इतर महसूलवाढीचे उपक्रम.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class SchemeApplicationsController : ControllerBase
{
    private readonly ISchemeApplicationService _service;
    private readonly ICurrentUserService _currentUser;

    public SchemeApplicationsController(ISchemeApplicationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] SchemeFilter filter) =>
        Ok(ApiResponse<PagedResult<SchemeApplicationDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound(ApiResponse<object>.Fail("अर्ज सापडला नाही.")) : Ok(ApiResponse<SchemeApplicationDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Create(CreateSchemeApplicationDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "System");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<SchemeApplicationDto>.Ok(created, "अर्ज यशस्वीरित्या नोंदवला."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Update(int id, UpdateSchemeApplicationDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "अर्ज अद्ययावत झाला.")) : NotFound(ApiResponse<object>.Fail("अर्ज सापडला नाही."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "अर्ज हटवला.")) : NotFound(ApiResponse<object>.Fail("अर्ज सापडला नाही."));
    }
}
