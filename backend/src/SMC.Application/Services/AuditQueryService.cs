using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;

namespace SMC.Application.Services;

public class AuditFilter : PagedRequest
{
    public string? EntityName { get; set; }
    public int? EntityId { get; set; }
    public string? UserName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public interface IAuditQueryService
{
    Task<PagedResult<AuditLogDto>> GetAllAsync(AuditFilter filter);
    Task<List<AuditLogDto>> GetForEntityAsync(string entityName, int entityId);
}

public class AuditQueryService : IAuditQueryService
{
    private readonly IApplicationDbContext _db;

    public AuditQueryService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static AuditLogDto ToDto(Domain.Entities.AuditLog a) => new()
    {
        Id = a.Id, UserName = a.UserName, Action = a.Action, EntityName = a.EntityName, EntityId = a.EntityId,
        FieldName = a.FieldName, OldValue = a.OldValue, NewValue = a.NewValue, Timestamp = a.Timestamp, IpAddress = a.IpAddress
    };

    public async Task<PagedResult<AuditLogDto>> GetAllAsync(AuditFilter filter)
    {
        var query = _db.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.EntityName)) query = query.Where(a => a.EntityName == filter.EntityName);
        if (filter.EntityId.HasValue) query = query.Where(a => a.EntityId == filter.EntityId);
        if (!string.IsNullOrWhiteSpace(filter.UserName)) query = query.Where(a => a.UserName.Contains(filter.UserName));
        if (filter.FromDate.HasValue) query = query.Where(a => a.Timestamp >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(a => a.Timestamp <= filter.ToDate);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(a => a.EntityName.Contains(term) || a.UserName.Contains(term)
                || a.Action.Contains(term) || a.EntityId.ToString().Contains(term)
                || (a.FieldName != null && a.FieldName.Contains(term)));
        }

        query = query.OrderByDescending(a => a.Timestamp);
        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        return new PagedResult<AuditLogDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    public async Task<List<AuditLogDto>> GetForEntityAsync(string entityName, int entityId)
    {
        var logs = await _db.AuditLogs.AsNoTracking().Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp).ToListAsync();
        return logs.Select(ToDto).ToList();
    }
}
