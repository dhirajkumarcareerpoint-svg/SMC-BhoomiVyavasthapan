using SMC.Application.Interfaces;
using SMC.Domain.Entities;

namespace SMC.Application.Services;

/// <summary>प्रत्येक Create/Update/Delete/Login कृतीचा इतिहास AuditLog मध्ये साठवते.</summary>
public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string action, string entityName, int entityId, string? fieldName = null,
        string? oldValue = null, string? newValue = null)
    {
        // AuditLog has a required FK to a staff User. Public applicant actions
        // have no authenticated UserId, so recording UserId=0 would fail the
        // request after its business data has already been saved.
        if (!_currentUser.UserId.HasValue)
            return;

        var log = new AuditLog
        {
            UserId = _currentUser.UserId.Value,
            UserName = _currentUser.UserName ?? "System",
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            Timestamp = DateTime.UtcNow,
            IpAddress = _currentUser.IpAddress
        };
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
