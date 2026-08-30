using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public class CalculationFilter : PagedRequest
{
    public string? Status { get; set; }
    public int? PropertyId { get; set; }
}

public interface ICalculationService
{
    Task<PagedResult<CalculationDto>> GetAllAsync(CalculationFilter filter);
    Task<CalculationDto?> GetByIdAsync(int id);
    Task<CalculationDto> CreateAsync(CreateCalculationDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdateCalculationDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

/// <summary>
/// Calculation (गणना) विभागासाठी CRUD सेवा.
///
/// टीप: CalculatedAmount / TotalAmount यांचे स्वयंचलित गणित (auto-calculation) करणारे
/// निश्चित व्यवसाय सूत्र प्रणालीमध्ये अद्याप उपलब्ध नाही (विद्यमान Property/Lease/RecoveryCase
/// entities मध्ये असे सूत्र परिभाषित केलेले नाही). त्यामुळे या सेवेत रक्कम शोधक (invent) न करता
/// अधिकाऱ्याने भरलेली/पडताळलेली रक्कमच जतन केली जाते. व्यवसाय सूत्र निश्चित झाल्यावर येथे
/// (उदा. CreateAsync/UpdateAsync मध्ये) संबंधित गणिती लॉजिक जोडता येईल.
/// </summary>
public class CalculationService : ICalculationService
{
    private readonly IApplicationDbContext _db;
    public CalculationService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static CalculationDto ToDto(Calculation c) => new()
    {
        Id = c.Id,
        PropertyId = c.PropertyId,
        PropertyName = c.Property?.Name,
        PropertyCode = c.Property?.PropertyCode,
        PropertyCategory = c.Property?.Category.ToString(),
        AreaSqFt = c.Property?.AreaSqFt,
        Rate = c.Rate,
        PeriodMonths = c.PeriodMonths,
        PreviousOutstanding = c.PreviousOutstanding,
        CurrentDemand = c.CurrentDemand,
        CalculatedAmount = c.CalculatedAmount,
        TotalAmount = c.TotalAmount,
        CalculationDate = c.CalculationDate,
        Status = c.Status.ToString(),
        Shera = c.Shera,
        CreatedBy = c.CreatedBy,
        CreatedAt = c.CreatedAt,
        UpdatedBy = c.UpdatedBy,
        UpdatedAt = c.UpdatedAt
    };

    public async Task<PagedResult<CalculationDto>> GetAllAsync(CalculationFilter filter)
    {
        var query = _db.Calculations.AsNoTracking().Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<CalculationStatus>(filter.Status, out var status))
            query = query.Where(c => c.Status == status);
        if (filter.PropertyId.HasValue)
            query = query.Where(c => c.PropertyId == filter.PropertyId);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(c =>
                (c.Property != null && c.Property.Name.Contains(term)) ||
                (c.Property != null && c.Property.PropertyCode.Contains(term)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CalculationDto
            {
                Id = c.Id,
                PropertyId = c.PropertyId,
                PropertyName = c.Property!.Name,
                PropertyCode = c.Property!.PropertyCode,
                PropertyCategory = c.Property.Category.ToString(),
                AreaSqFt = c.Property.AreaSqFt,
                Rate = c.Rate,
                PeriodMonths = c.PeriodMonths,
                PreviousOutstanding = c.PreviousOutstanding,
                CurrentDemand = c.CurrentDemand,
                CalculatedAmount = c.CalculatedAmount,
                TotalAmount = c.TotalAmount,
                CalculationDate = c.CalculationDate,
                Status = c.Status.ToString(),
                Shera = c.Shera,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedBy = c.UpdatedBy,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<CalculationDto> { Items = items, TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    public async Task<CalculationDto?> GetByIdAsync(int id)
    {
        var c = await _db.Calculations.AsNoTracking().Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return c is null ? null : ToDto(c);
    }

    public async Task<CalculationDto> CreateAsync(CreateCalculationDto dto, string createdBy)
    {
        var entity = new Calculation
        {
            PropertyId = dto.PropertyId,
            Rate = dto.Rate,
            PeriodMonths = dto.PeriodMonths,
            PreviousOutstanding = dto.PreviousOutstanding,
            CurrentDemand = dto.CurrentDemand,
            CalculatedAmount = dto.CalculatedAmount,
            TotalAmount = dto.TotalAmount,
            CalculationDate = dto.CalculationDate,
            Status = Enum.Parse<CalculationStatus>(dto.Status),
            Shera = dto.Shera,
            CreatedBy = createdBy
        };
        _db.Calculations.Add(entity);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(entity.Id) ?? throw new InvalidOperationException("नवीन गणना नोंद सापडली नाही.");
    }

    public async Task<bool> UpdateAsync(int id, UpdateCalculationDto dto, string updatedBy)
    {
        var entity = await _db.Calculations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;

        entity.PropertyId = dto.PropertyId;
        entity.Rate = dto.Rate;
        entity.PeriodMonths = dto.PeriodMonths;
        entity.PreviousOutstanding = dto.PreviousOutstanding;
        entity.CurrentDemand = dto.CurrentDemand;
        entity.CalculatedAmount = dto.CalculatedAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.CalculationDate = dto.CalculationDate;
        entity.Status = Enum.Parse<CalculationStatus>(dto.Status);
        entity.Shera = dto.Shera;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var entity = await _db.Calculations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
