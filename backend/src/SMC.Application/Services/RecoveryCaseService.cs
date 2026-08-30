using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public class RecoveryFilter : PagedRequest
{
    public string? Stage { get; set; }
    public int? PropertyId { get; set; }
}

public interface IRecoveryCaseService
{
    Task<PagedResult<RecoveryCaseDto>> GetAllAsync(RecoveryFilter filter);
    Task<RecoveryCaseDto?> GetByIdAsync(int id);
    Task<RecoveryCaseDto> CreateAsync(CreateRecoveryCaseDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdateRecoveryCaseDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

public class RecoveryCaseService : IRecoveryCaseService
{
    private readonly IApplicationDbContext _db;
    public RecoveryCaseService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static RecoveryCaseDto ToDto(RecoveryCase r) => new()
    {
        Id = r.Id,
        PropertyId = r.PropertyId,
        PropertyName = r.Property?.Name,
        PropertyCode = r.Property?.PropertyCode,
        LeaseId = r.LeaseId,
        LesseeName = r.Lease?.LesseeName,
        MonthsOverdue = r.MonthsOverdue,
        OutstandingAmount = r.OutstandingAmount,
        Stage = r.Stage.ToString(),
        NoticeNumber = r.NoticeNumber,
        NoticeDate = r.NoticeDate,
        RecoveredAmount = r.RecoveredAmount,
        RecoveryDate = r.RecoveryDate,
        SealDate = r.SealDate,
        ReAuctionDate = r.ReAuctionDate,
        Shera = r.Shera,
        CreatedBy = r.CreatedBy,
        CreatedAt = r.CreatedAt,
        UpdatedBy = r.UpdatedBy,
        UpdatedAt = r.UpdatedAt
    };

    public async Task<PagedResult<RecoveryCaseDto>> GetAllAsync(RecoveryFilter filter)
    {
        var query = _db.RecoveryCases.AsNoTracking().Include(r => r.Property).Include(r => r.Lease).Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Stage) && Enum.TryParse<RecoveryStage>(filter.Stage, out var stage))
            query = query.Where(r => r.Stage == stage);
        if (filter.PropertyId.HasValue)
            query = query.Where(r => r.PropertyId == filter.PropertyId);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => (r.NoticeNumber != null && r.NoticeNumber.Contains(term))
                || (r.Property != null && r.Property.Name.Contains(term))
                || (r.Lease != null && r.Lease.LesseeName.Contains(term)));
        }

        query = query.OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return new PagedResult<RecoveryCaseDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    public async Task<RecoveryCaseDto?> GetByIdAsync(int id)
    {
        var r = await _db.RecoveryCases.AsNoTracking().Include(x => x.Property).Include(x => x.Lease)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return r is null ? null : ToDto(r);
    }

    public async Task<RecoveryCaseDto> CreateAsync(CreateRecoveryCaseDto dto, string createdBy)
    {
        if (dto.MonthsOverdue < 3)
            throw new InvalidOperationException("वसुली प्रकरण नोंदविण्यासाठी किमान 3 महिने भाडे थकीत असणे आवश्यक आहे.");

        var entity = new RecoveryCase
        {
            PropertyId = dto.PropertyId,
            LeaseId = dto.LeaseId,
            MonthsOverdue = dto.MonthsOverdue,
            OutstandingAmount = dto.OutstandingAmount,
            Stage = Enum.Parse<RecoveryStage>(dto.Stage),
            NoticeNumber = dto.NoticeNumber,
            NoticeDate = dto.NoticeDate,
            RecoveredAmount = dto.RecoveredAmount,
            RecoveryDate = dto.RecoveryDate,
            SealDate = dto.SealDate,
            ReAuctionDate = dto.ReAuctionDate,
            Shera = dto.Shera,
            CreatedBy = createdBy
        };
        _db.RecoveryCases.Add(entity);

        if (entity.Stage == RecoveryStage.Seal)
        {
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == dto.PropertyId);
            if (property is not null) { property.Status = PropertyStatus.Seal; property.UpdatedBy = createdBy; property.UpdatedAt = DateTime.UtcNow; }
        }

        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateRecoveryCaseDto dto, string updatedBy)
    {
        var entity = await _db.RecoveryCases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;

        var before = new RecoveryCase
        {
            MonthsOverdue = entity.MonthsOverdue, OutstandingAmount = entity.OutstandingAmount, Stage = entity.Stage,
            NoticeNumber = entity.NoticeNumber, NoticeDate = entity.NoticeDate, RecoveredAmount = entity.RecoveredAmount,
            RecoveryDate = entity.RecoveryDate, SealDate = entity.SealDate, ReAuctionDate = entity.ReAuctionDate, Shera = entity.Shera
        };

        entity.MonthsOverdue = dto.MonthsOverdue;
        entity.OutstandingAmount = dto.OutstandingAmount;
        entity.Stage = Enum.Parse<RecoveryStage>(dto.Stage);
        entity.NoticeNumber = dto.NoticeNumber;
        entity.NoticeDate = dto.NoticeDate;
        entity.RecoveredAmount = dto.RecoveredAmount;
        entity.RecoveryDate = dto.RecoveryDate;
        entity.SealDate = dto.SealDate;
        entity.ReAuctionDate = dto.ReAuctionDate;
        entity.Shera = dto.Shera;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        // टप्पा (Stage) नुसार मालमत्तेची स्थिती स्वयंचलित अद्ययावत करा.
        var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == entity.PropertyId);
        if (property is not null)
        {
            if (entity.Stage == RecoveryStage.Seal) property.Status = PropertyStatus.Seal;
            else if (entity.Stage == RecoveryStage.Punarlilaw) property.Status = PropertyStatus.Punarlilaw;
            property.UpdatedBy = updatedBy;
            property.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var entity = await _db.RecoveryCases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
