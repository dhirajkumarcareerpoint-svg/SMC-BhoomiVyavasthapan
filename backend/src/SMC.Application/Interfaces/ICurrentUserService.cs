namespace SMC.Application.Interfaces;

/// <summary>JWT token मधून सध्याचा login झालेला user मिळवण्यासाठी.</summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    string? Role { get; }
    string? IpAddress { get; }
}
