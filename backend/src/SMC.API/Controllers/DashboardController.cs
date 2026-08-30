using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSummary() =>
        Ok(ApiResponse<DashboardSummaryDto>.Ok(await _service.GetSummaryAsync()));
}
