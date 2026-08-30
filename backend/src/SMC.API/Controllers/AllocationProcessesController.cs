using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>मालमत्ता देण्याची कार्यपद्धती (Tab 5) - सार्वजनिक लिलाव, निविदा, प्रसिद्धीकरण करून अर्ज मागविणे.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class AllocationProcessesController : ControllerBase
{
    private readonly IAllocationProcessService _service;
    private readonly ICurrentUserService _currentUser;

    public AllocationProcessesController(IAllocationProcessService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] AllocationFilter filter) =>
        Ok(ApiResponse<PagedResult<AllocationProcessDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound(ApiResponse<object>.Fail("वाटप प्रक्रिया सापडली नाही.")) : Ok(ApiResponse<AllocationProcessDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Create(CreateAllocationProcessDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "System");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<AllocationProcessDto>.Ok(created, "वाटप प्रक्रिया यशस्वीरित्या नोंदवली."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Update(int id, UpdateAllocationProcessDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "वाटप प्रक्रिया अद्ययावत झाली.")) : NotFound(ApiResponse<object>.Fail("नोंद सापडली नाही."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "नोंद हटवली.")) : NotFound(ApiResponse<object>.Fail("नोंद सापडली नाही."));
    }
}
