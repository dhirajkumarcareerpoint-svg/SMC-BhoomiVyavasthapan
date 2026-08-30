namespace SMC.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "Staff";
}

public class UpdateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
}
