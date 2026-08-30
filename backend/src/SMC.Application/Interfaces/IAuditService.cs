namespace SMC.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, int entityId, string? fieldName = null,
        string? oldValue = null, string? newValue = null);
}
