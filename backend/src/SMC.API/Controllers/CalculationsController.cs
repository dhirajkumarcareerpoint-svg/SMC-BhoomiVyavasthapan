using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>Calculation (गणना) - निवडलेल्या मालमत्तेसाठी भाडे/शुल्क आकारणीची नोंद.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class CalculationsController : ControllerBase
{
    private readonly ICalculationService _service;
    private readonly ICurrentUserService _currentUser;

    public CalculationsController(ICalculationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CalculationFilter filter) =>
        Ok(ApiResponse<PagedResult<CalculationDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound(ApiResponse<object>.Fail("गणना नोंद सापडली नाही.")) : Ok(ApiResponse<CalculationDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Create(CreateCalculationDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "System");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CalculationDto>.Ok(created, "गणना यशस्वीरित्या नोंदवली."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOrOfficer")]
    public async Task<IActionResult> Update(int id, UpdateCalculationDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "गणना अद्ययावत झाली.")) : NotFound(ApiResponse<object>.Fail("गणना नोंद सापडली नाही."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "System");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "गणना हटवली.")) : NotFound(ApiResponse<object>.Fail("गणना नोंद सापडली नाही."));
    }
}
