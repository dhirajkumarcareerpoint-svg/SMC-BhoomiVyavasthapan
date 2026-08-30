using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>वसुली प्रक्रिया (Tab 3) - थकबाकी → नोटीस → वसुली → सील → पुनर्लिलाव.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class RecoveryCasesController : ControllerBase
{
    private readonly IRecoveryCaseService _service;
    private readonly ICurrentUserService _currentUser;

    public RecoveryCasesController(IRecoveryCaseService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] RecoveryFilter filter) =>
        Ok(ApiResponse<PagedResult<RecoveryCaseDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound(ApiResponse<object>.Fail("वसुली प्रकरण सापडले नाही.")) : Ok(ApiResponse<RecoveryCaseDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Create(CreateRecoveryCaseDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "System");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<RecoveryCaseDto>.Ok(created, "वसुली प्रकरण यशस्वीरित्या नोंदवले."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Update(int id, UpdateRecoveryCaseDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "वसुली प्रकरण अद्ययावत झाले.")) : NotFound(ApiResponse<object>.Fail("प्रकरण सापडले नाही."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "वसुली प्रकरण हटवले.")) : NotFound(ApiResponse<object>.Fail("प्रकरण सापडले नाही."));
    }
}
