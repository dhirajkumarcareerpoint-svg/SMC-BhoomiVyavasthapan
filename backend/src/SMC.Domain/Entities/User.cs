using SMC.Domain.Common;
using SMC.Domain.Enums;

namespace SMC.Domain.Entities;

/// <summary>
/// प्रणाली वापरकर्ता (Admin / Officer / Staff). एकूण 10 कर्मचारी login साठी seed केले जातात.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;      // पूर्ण नाव
    public string? Designation { get; set; }                   // पदनाम
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
