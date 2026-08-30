namespace SMC.Domain.Common;

/// <summary>
/// प्रत्येक Entity साठी समान audit fields (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, Soft Delete)
/// आणि सामायिक शेरा (Remarks) फील्ड.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    /// <summary>शेरा (Remarks) - प्रत्येक मुख्य entity ला असणारा सामायिक remarks text box.</summary>
    public string? Shera { get; set; }
}
