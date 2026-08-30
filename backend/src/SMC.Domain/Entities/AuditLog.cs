namespace SMC.Domain.Entities;

/// <summary>
/// प्रत्येक Create/Update/Delete action चा संपूर्ण इतिहास: कोणी, काय, जुने मूल्य, नवीन मूल्य, तारीख-वेळ.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public string UserName { get; set; } = string.Empty;        // नोंदीच्या वेळेचे snapshot नाव

    public string Action { get; set; } = string.Empty;          // Create / Update / Delete / Login
    public string EntityName { get; set; } = string.Empty;      // उदा. Property, Lease
    public int EntityId { get; set; }

    public string? FieldName { get; set; }                      // बदललेले फील्ड
    public string? OldValue { get; set; }                       // जुनी value
    public string? NewValue { get; set; }                       // नवीन value

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
