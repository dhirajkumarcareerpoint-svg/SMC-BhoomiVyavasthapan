using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public class SchemeFilter : PagedRequest
{
    public string? SchemeType { get; set; }
    public string? Status { get; set; }
}

public interface ISchemeApplicationService
{
    Task<PagedResult<SchemeApplicationDto>> GetAllAsync(SchemeFilter filter);
    Task<SchemeApplicationDto?> GetByIdAsync(int id);
    Task<SchemeApplicationDto> CreateAsync(CreateSchemeApplicationDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdateSchemeApplicationDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

public class SchemeApplicationService : ISchemeApplicationService
{
    private readonly IApplicationDbContext _db;
    public SchemeApplicationService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static SchemeApplicationDto ToDto(SchemeApplication s) => new()
    {
        Id = s.Id, PropertyId = s.PropertyId, PropertyName = s.Property?.Name,
        SchemeType = s.SchemeType.ToString(), ApplicantName = s.ApplicantName, ApplicantMobile = s.ApplicantMobile,
        ApplicationDate = s.ApplicationDate, OriginalOutstanding = s.OriginalOutstanding, WaivedAmount = s.WaivedAmount,
        PayableAmount = s.PayableAmount, Status = s.Status.ToString(), DecisionDate = s.DecisionDate,
        ApprovedBy = s.ApprovedBy, Shera = s.Shera, CreatedBy = s.CreatedBy, CreatedAt = s.CreatedAt,
        UpdatedBy = s.UpdatedBy, UpdatedAt = s.UpdatedAt
    };

    public async Task<PagedResult<SchemeApplicationDto>> GetAllAsync(SchemeFilter filter)
    {
        var query = _db.SchemeApplications.AsNoTracking().Include(s => s.Property).Where(s => !s.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filter.SchemeType) && Enum.TryParse<SchemeType>(filter.SchemeType, out var t))
            query = query.Where(s => s.SchemeType == t);
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<SchemeStatus>(filter.Status, out var st))
            query = query.Where(s => s.Status == st);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(s => s.ApplicantName.Contains(term) || (s.Property != null && s.Property.Name.Contains(term)));
        }
        query = query.OrderByDescending(s => s.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        return new PagedResult<SchemeApplicationDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    public async Task<SchemeApplicationDto?> GetByIdAsync(int id)
    {
        var s = await _db.SchemeApplications.AsNoTracking().Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return s is null ? null : ToDto(s);
    }

    public async Task<SchemeApplicationDto> CreateAsync(CreateSchemeApplicationDto dto, string createdBy)
    {
        var entity = new SchemeApplication
        {
            PropertyId = dto.PropertyId, SchemeType = Enum.Parse<SchemeType>(dto.SchemeType),
            ApplicantName = dto.ApplicantName, ApplicantMobile = dto.ApplicantMobile, ApplicationDate = dto.ApplicationDate,
            OriginalOutstanding = dto.OriginalOutstanding, WaivedAmount = dto.WaivedAmount, PayableAmount = dto.PayableAmount,
            Status = Enum.Parse<SchemeStatus>(dto.Status), DecisionDate = dto.DecisionDate, ApprovedBy = dto.ApprovedBy,
            Shera = dto.Shera, CreatedBy = createdBy
        };
        _db.SchemeApplications.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateSchemeApplicationDto dto, string updatedBy)
    {
        var entity = await _db.SchemeApplications.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;

        var before = new SchemeApplication
        {
            SchemeType = entity.SchemeType, ApplicantName = entity.ApplicantName, OriginalOutstanding = entity.OriginalOutstanding,
            WaivedAmount = entity.WaivedAmount, PayableAmount = entity.PayableAmount, Status = entity.Status, Shera = entity.Shera
        };

        entity.SchemeType = Enum.Parse<SchemeType>(dto.SchemeType);
        entity.ApplicantName = dto.ApplicantName;
        entity.ApplicantMobile = dto.ApplicantMobile;
        entity.ApplicationDate = dto.ApplicationDate;
        entity.OriginalOutstanding = dto.OriginalOutstanding;
        entity.WaivedAmount = dto.WaivedAmount;
        entity.PayableAmount = dto.PayableAmount;
        entity.Status = Enum.Parse<SchemeStatus>(dto.Status);
        entity.DecisionDate = dto.DecisionDate;
        entity.ApprovedBy = dto.ApprovedBy;
        entity.Shera = dto.Shera;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var entity = await _db.SchemeApplications.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
