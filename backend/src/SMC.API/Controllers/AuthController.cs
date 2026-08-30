using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>Login: वापरकर्तानाव आणि पासवर्ड देऊन JWT token मिळवा.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result is null)
            return Unauthorized(ApiResponse<object>.Fail("वापरकर्तानाव किंवा पासवर्ड चुकीचा आहे."));

        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "यशस्वीरित्या लॉगिन झाले."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        var ok = await _authService.ChangePasswordAsync(_currentUser.UserId.Value, dto);
        if (!ok) return BadRequest(ApiResponse<object>.Fail("सध्याचा पासवर्ड चुकीचा आहे."));
        return Ok(ApiResponse<object>.Ok(new { }, "पासवर्ड यशस्वीरित्या बदलला."));
    }
}
