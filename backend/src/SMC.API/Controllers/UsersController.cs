using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

/// <summary>User व्यवस्थापन - फक्त Admin. (10 staff login तयार/संपादन करण्यासाठी).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUserService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<List<UserDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound(ApiResponse<object>.Fail("वापरकर्ता सापडला नाही.")) : Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        var created = await _service.CreateAsync(dto, _currentUser.UserName ?? "Admin");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<UserDto>.Ok(created, "वापरकर्ता तयार झाला."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto, _currentUser.UserName ?? "Admin");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "अद्ययावत झाले.")) : NotFound(ApiResponse<object>.Fail("वापरकर्ता सापडला नाही."));
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
    {
        var ok = await _service.ResetPasswordAsync(id, newPassword, _currentUser.UserName ?? "Admin");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "पासवर्ड रीसेट झाला.")) : NotFound(ApiResponse<object>.Fail("वापरकर्ता सापडला नाही."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id, _currentUser.UserName ?? "Admin");
        return ok ? Ok(ApiResponse<object>.Ok(new { }, "वापरकर्ता निष्क्रिय केला.")) : NotFound(ApiResponse<object>.Fail("वापरकर्ता सापडला नाही."));
    }
}
