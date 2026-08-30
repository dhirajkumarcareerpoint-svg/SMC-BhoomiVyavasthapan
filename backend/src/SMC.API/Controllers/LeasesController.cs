using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>हस्तांतरण (Tab 2) - दस्ताद्वारे भाडेपट्टा.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class LeasesController : ControllerBase
{
    private readonly ILeaseService _service;
    private readonly ICurrentUserService _currentUser;

    public LeasesController(ILeaseService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] LeaseFilter filter) =>
        Ok(ApiResponse<PagedResult<LeaseDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound(ApiResponse<object>.Fail("हस्तांतरण नोंद सापडली नाही.")) : Ok(ApiResponse<LeaseDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Create(CreateLeaseDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "System");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<LeaseDto>.Ok(created, "हस्तांतरण/भाडेपट्टा यशस्वीरित्या नोंदवला."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Update(int id, UpdateLeaseDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "हस्तांतरण नोंद अद्ययावत झाली.")) : NotFound(ApiResponse<object>.Fail("नोंद सापडली नाही."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "हस्तांतरण नोंद हटवली.")) : NotFound(ApiResponse<object>.Fail("नोंद सापडली नाही."));
    }
}
