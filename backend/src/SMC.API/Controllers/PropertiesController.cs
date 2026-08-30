using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>मालमत्ता (Tab 1) - Major/Mini गाळे, Land Fee, समाज मंदिर, अभ्यासिका, 256 गाळे, TP-3/23, अधिकृत खोके, इतर.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _service;
    private readonly ICurrentUserService _currentUser;

    public PropertiesController(IPropertyService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PropertyFilter filter) =>
        Ok(ApiResponse<PagedResult<PropertyDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("next-code")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNextCode([FromQuery] string category) =>
        Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(category)));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound(ApiResponse<object>.Fail("मालमत्ता सापडली नाही.")) : Ok(ApiResponse<PropertyDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Create(CreatePropertyDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "System");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<PropertyDto>.Ok(created, "मालमत्ता यशस्वीरित्या नोंदवली."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Update(int id, UpdatePropertyDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "मालमत्ता अद्ययावत झाली.")) : NotFound(ApiResponse<object>.Fail("मालमत्ता सापडली नाही."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "मालमत्ता हटवली.")) : NotFound(ApiResponse<object>.Fail("मालमत्ता सापडली नाही."));
    }
}
