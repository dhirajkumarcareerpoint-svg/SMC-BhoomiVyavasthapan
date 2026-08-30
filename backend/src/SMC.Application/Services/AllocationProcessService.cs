using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public class AllocationFilter : PagedRequest
{
    public string? Method { get; set; }
    public string? Status { get; set; }
}

public interface IAllocationProcessService
{
    Task<PagedResult<AllocationProcessDto>> GetAllAsync(AllocationFilter filter);
    Task<AllocationProcessDto?> GetByIdAsync(int id);
    Task<AllocationProcessDto> CreateAsync(CreateAllocationProcessDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdateAllocationProcessDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

public class AllocationProcessService : IAllocationProcessService
{
    private readonly IApplicationDbContext _db;
    public AllocationProcessService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static AllocationProcessDto ToDto(AllocationProcess a) => new()
    {
        Id = a.Id, PropertyId = a.PropertyId, PropertyName = a.Property?.Name, Method = a.Method.ToString(),
        NoticeNumber = a.NoticeNumber, PublishDate = a.PublishDate, LastDateToApply = a.LastDateToApply,
        AuctionDate = a.AuctionDate, ReserveAmount = a.ReserveAmount, HighestBidAmount = a.HighestBidAmount,
        HighestBidderName = a.HighestBidderName, HighestBidderMobile = a.HighestBidderMobile, Status = a.Status.ToString(),
        Shera = a.Shera, CreatedBy = a.CreatedBy, CreatedAt = a.CreatedAt, UpdatedBy = a.UpdatedBy, UpdatedAt = a.UpdatedAt
    };

    public async Task<PagedResult<AllocationProcessDto>> GetAllAsync(AllocationFilter filter)
    {
        var query = _db.AllocationProcesses.AsNoTracking().Include(a => a.Property).Where(a => !a.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filter.Method) && Enum.TryParse<AllocationMethod>(filter.Method, out var m))
            query = query.Where(a => a.Method == m);
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<AllocationStatus>(filter.Status, out var st))
            query = query.Where(a => a.Status == st);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(a => (a.NoticeNumber != null && a.NoticeNumber.Contains(term))
                || (a.HighestBidderName != null && a.HighestBidderName.Contains(term))
                || (a.Property != null && a.Property.Name.Contains(term)));
        }
        query = query.OrderByDescending(a => a.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        return new PagedResult<AllocationProcessDto> { Items = items.Select(ToDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    public async Task<AllocationProcessDto?> GetByIdAsync(int id)
    {
        var a = await _db.AllocationProcesses.AsNoTracking().Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return a is null ? null : ToDto(a);
    }

    public async Task<AllocationProcessDto> CreateAsync(CreateAllocationProcessDto dto, string createdBy)
    {
        var entity = new AllocationProcess
        {
            PropertyId = dto.PropertyId, Method = Enum.Parse<AllocationMethod>(dto.Method), NoticeNumber = dto.NoticeNumber,
            PublishDate = dto.PublishDate, LastDateToApply = dto.LastDateToApply, AuctionDate = dto.AuctionDate,
            ReserveAmount = dto.ReserveAmount, HighestBidAmount = dto.HighestBidAmount, HighestBidderName = dto.HighestBidderName,
            HighestBidderMobile = dto.HighestBidderMobile, Status = Enum.Parse<AllocationStatus>(dto.Status),
            Shera = dto.Shera, CreatedBy = createdBy
        };
        _db.AllocationProcesses.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateAllocationProcessDto dto, string updatedBy)
    {
        var entity = await _db.AllocationProcesses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;

        var before = new AllocationProcess
        {
            Method = entity.Method, NoticeNumber = entity.NoticeNumber, HighestBidAmount = entity.HighestBidAmount,
            HighestBidderName = entity.HighestBidderName, Status = entity.Status, Shera = entity.Shera
        };

        entity.Method = Enum.Parse<AllocationMethod>(dto.Method);
        entity.NoticeNumber = dto.NoticeNumber;
        entity.PublishDate = dto.PublishDate;
        entity.LastDateToApply = dto.LastDateToApply;
        entity.AuctionDate = dto.AuctionDate;
        entity.ReserveAmount = dto.ReserveAmount;
        entity.HighestBidAmount = dto.HighestBidAmount;
        entity.HighestBidderName = dto.HighestBidderName;
        entity.HighestBidderMobile = dto.HighestBidderMobile;
        entity.Status = Enum.Parse<AllocationStatus>(dto.Status);
        entity.Shera = dto.Shera;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        // मंजूर झाल्यास सर्वाधिक बोली लावणाऱ्या पात्र व्यक्तीला मालमत्ता वाटप नोंद अद्ययावत करा.
        if (entity.Status == AllocationStatus.Manjur)
        {
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == entity.PropertyId);
            if (property is not null)
            {
                property.Status = PropertyStatus.Bhadyane;
                property.CurrentOccupant = entity.HighestBidderName;
                property.UpdatedBy = updatedBy;
                property.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var entity = await _db.AllocationProcesses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
