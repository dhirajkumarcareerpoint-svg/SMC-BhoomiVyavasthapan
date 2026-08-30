using System.Reflection;

namespace SMC.Application.Common;

/// <summary>
/// Update करण्यापूर्वीचा snapshot आणि नंतरची entity यामधील बदललेली fields शोधून
/// AuditLog साठी (FieldName, OldValue, NewValue) यादी तयार करते.
/// </summary>
public static class AuditDiffHelper
{
    private static readonly HashSet<string> Ignored = new()
    {
        "Id", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
        "IsDeleted", "DeletedAt", "DeletedBy"
    };

    public static List<(string Field, string? OldValue, string? NewValue)> GetChanges<T>(T before, T after)
    {
        var changes = new List<(string, string?, string?)>();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !Ignored.Contains(p.Name)
                        && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                        && p.GetIndexParameters().Length == 0);

        foreach (var prop in props)
        {
            var oldVal = prop.GetValue(before)?.ToString();
            var newVal = prop.GetValue(after)?.ToString();
            if (oldVal != newVal)
                changes.Add((prop.Name, oldVal, newVal));
        }
        return changes;
    }
}
