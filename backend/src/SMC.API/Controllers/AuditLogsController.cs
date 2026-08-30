using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>Audit History - सर्व users ना records आणि activity history दिसते.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditQueryService _service;

    public AuditLogsController(IAuditQueryService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] AuditFilter filter) =>
        Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(await _service.GetAllAsync(filter)));

    [HttpGet("entity")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForEntity([FromQuery] string entityName, [FromQuery] int entityId) =>
        Ok(ApiResponse<List<AuditLogDto>>.Ok(await _service.GetForEntityAsync(entityName, entityId)));
}
