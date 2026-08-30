using Microsoft.EntityFrameworkCore;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdateUserDto dto, string updatedBy);
    Task<bool> ResetPasswordAsync(int id, string newPassword, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

public class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;

    public UserService(IApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Designation = u.Designation,
        Mobile = u.Mobile,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt
    };

    public async Task<List<UserDto>> GetAllAsync() =>
        await _db.Users.AsNoTracking().Where(u => !u.IsDeleted).OrderBy(u => u.FullName)
            .Select(u => ToDto(u)).ToListAsync();

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return u is null ? null : ToDto(u);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, string createdBy)
    {
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Designation = dto.Designation,
            Mobile = dto.Mobile,
            Email = dto.Email,
            Role = Enum.Parse<UserRole>(dto.Role),
            IsActive = true,
            CreatedBy = createdBy
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto, string updatedBy)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null) return false;

        user.FullName = dto.FullName;
        user.Designation = dto.Designation;
        user.Mobile = dto.Mobile;
        user.Email = dto.Email;
        user.Role = Enum.Parse<UserRole>(dto.Role);
        user.IsActive = dto.IsActive;
        user.UpdatedBy = updatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword, string updatedBy)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedBy = updatedBy;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Update", nameof(User), user.Id, "Password", "******", "****** (reset)");
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null) return false;
        user.IsDeleted = true;
        user.DeletedBy = deletedBy;
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
