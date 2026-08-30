using Microsoft.EntityFrameworkCore;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;

namespace SMC.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _audit;

    public AuthService(IApplicationDbContext db, ITokenService tokenService, IAuditService audit)
    {
        _db = db;
        _tokenService = tokenService;
        _audit = audit;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var username = dto.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(dto.Password))
            return null;

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            !u.IsDeleted && u.Username.ToUpper() == username.ToUpper());

        if (user is null || !user.IsActive) return null;
        // Passwords are deliberately not trimmed or normalized: BCrypt must
        // verify exactly what the user entered.
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            UserId = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user is null) return false;
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash)) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Update", nameof(User), user.Id, "Password", "******", "******");
        return true;
    }
}
