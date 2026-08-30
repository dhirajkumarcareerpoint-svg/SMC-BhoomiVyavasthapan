using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public class LeaseFilter : PagedRequest
{
    public string? DurationType { get; set; }
    public int? PropertyId { get; set; }
}

public interface ILeaseService
{
    Task<PagedResult<LeaseDto>> GetAllAsync(LeaseFilter filter);
    Task<LeaseDto?> GetByIdAsync(int id);
    Task<LeaseDto> CreateAsync(CreateLeaseDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdateLeaseDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

public class LeaseService : ILeaseService
{
    private readonly IApplicationDbContext _db;

    public LeaseService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<LeaseDto>> GetAllAsync(LeaseFilter filter)
    {
        var query = _db.Leases.AsNoTracking().Include(l => l.Property).Where(l => !l.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filter.DurationType) && Enum.TryParse<LeaseDurationType>(filter.DurationType, out var dt))
            query = query.Where(l => l.DurationType == dt);
        if (filter.PropertyId.HasValue)
            query = query.Where(l => l.PropertyId == filter.PropertyId);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(l => l.LesseeName.Contains(term) || l.DeedNumber.Contains(term)
                || (l.Property != null && l.Property.Name.Contains(term)));
        }

        query = query.OrderByDescending(l => l.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        return new PagedResult<LeaseDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    private static LeaseDto ToDto(Lease l) => new()
    {
        Id = l.Id,
        PropertyId = l.PropertyId,
        PropertyName = l.Property?.Name,
        PropertyCode = l.Property?.PropertyCode,
        LesseeName = l.LesseeName,
        LesseeMobile = l.LesseeMobile,
        LesseeAddress = l.LesseeAddress,
        DeedNumber = l.DeedNumber,
        DeedDate = l.DeedDate,
        DurationType = l.DurationType.ToString(),
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        RentAmount = l.RentAmount,
        Status = l.Status.ToString(),
        Shera = l.Shera,
        CreatedBy = l.CreatedBy,
        CreatedAt = l.CreatedAt,
        UpdatedBy = l.UpdatedBy,
        UpdatedAt = l.UpdatedAt
    };


    public async Task<LeaseDto?> GetByIdAsync(int id)
    {
        var l = await _db.Leases.AsNoTracking().Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return l is null ? null : ToDto(l);
    }

    public async Task<LeaseDto> CreateAsync(CreateLeaseDto dto, string createdBy)
    {
        var entity = new Lease
        {
            PropertyId = dto.PropertyId,
            LesseeName = dto.LesseeName,
            LesseeMobile = dto.LesseeMobile,
            LesseeAddress = dto.LesseeAddress,
            DeedDate = dto.DeedDate,
            DurationType = Enum.Parse<LeaseDurationType>(dto.DurationType),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            RentAmount = dto.RentAmount,
            SecurityDeposit = dto.SecurityDeposit,
            Status = Enum.Parse<LeaseStatus>(dto.Status),
            Shera = dto.Shera,
            CreatedBy = createdBy
        };
        _db.Leases.Add(entity);

        // हस्तांतरण नोंदल्यावर संबंधित मालमत्तेची स्थिती "भाडेतत्त्वावर दिलेली" करा.
        var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == dto.PropertyId);
        if (property is not null && entity.Status == LeaseStatus.Saru)
        {
            property.Status = PropertyStatus.Bhadyane;
            property.CurrentOccupant = dto.LesseeName;
            property.UpdatedBy = createdBy;
            property.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateLeaseDto dto, string updatedBy)
    {
        var entity = await _db.Leases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;

        var before = new Lease
        {
            PropertyId = entity.PropertyId, LesseeName = entity.LesseeName, LesseeMobile = entity.LesseeMobile,
            DeedNumber = entity.DeedNumber, DeedDate = entity.DeedDate, DurationType = entity.DurationType,
            StartDate = entity.StartDate, EndDate = entity.EndDate, RentAmount = entity.RentAmount,
            SecurityDeposit = entity.SecurityDeposit, Status = entity.Status, Shera = entity.Shera
        };

        entity.PropertyId = dto.PropertyId;
        entity.LesseeName = dto.LesseeName;
        entity.LesseeMobile = dto.LesseeMobile;
        entity.LesseeAddress = dto.LesseeAddress;
        entity.DeedNumber = dto.DeedNumber;
        entity.DeedDate = dto.DeedDate;
        entity.DurationType = Enum.Parse<LeaseDurationType>(dto.DurationType);
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.RentAmount = dto.RentAmount;
        entity.SecurityDeposit = dto.SecurityDeposit;
        entity.Status = Enum.Parse<LeaseStatus>(dto.Status);
        entity.Shera = dto.Shera;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var entity = await _db.Leases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
