using Microsoft.EntityFrameworkCore;
using SMC.Application.Common;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using SMC.Domain.Enums;
using System.Text.RegularExpressions;

namespace SMC.Application.Services;

public class PropertyFilter : PagedRequest
{
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Ward { get; set; }
}

public interface IPropertyService
{
    Task<PagedResult<PropertyDto>> GetAllAsync(PropertyFilter filter);
    Task<string> GetNextCodeAsync(string category);
    Task<PropertyDto?> GetByIdAsync(int id);
    Task<PropertyDto> CreateAsync(CreatePropertyDto dto, string createdBy);
    Task<bool> UpdateAsync(int id, UpdatePropertyDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
}

public class PropertyService : IPropertyService
{
    private readonly IApplicationDbContext _db;

    public PropertyService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static PropertyDto ToDto(Property p) => new()
    {
        Id = p.Id,
        Category = p.Category.ToString(),
        PropertyCode = p.PropertyCode,
        Name = p.Name,
        Ward = p.Ward,
        Zone = p.Zone,
        Address = p.Address,
        AreaSqFt = p.AreaSqFt,
        MonthlyRent = p.MonthlyRent,
        AnnualDemand = p.AnnualDemand,
        SurveyNumber = p.SurveyNumber,
        TpNumber = p.TpNumber,
        Status = p.Status.ToString(),
        CurrentOccupant = p.CurrentOccupant,
        Shera = p.Shera,
        CreatedBy = p.CreatedBy,
        CreatedAt = p.CreatedAt,
        UpdatedBy = p.UpdatedBy,
        UpdatedAt = p.UpdatedAt,
        DocumentCount = p.Documents?.Count(d => !d.IsDeleted) ?? 0
    };

    public async Task<string> GetNextCodeAsync(string category)
    {
        var prefix = GetPrefix(category);
        var codes = await _db.Properties.IgnoreQueryFilters().AsNoTracking()
            .Select(p => p.PropertyCode).ToListAsync();
        var next = codes
            .Select(code => Regex.Match(code, $"^{Regex.Escape(prefix)}-(\\d+)$"))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .DefaultIfEmpty(0)
            .Max() + 1;
        return $"{prefix}-{next:000}";
    }

    private static string GetPrefix(string category) => category switch
    {
        nameof(PropertyCategory.MajorGaale) => "MJ",
        nameof(PropertyCategory.MiniGaale) => "MN",
        nameof(PropertyCategory.LandFee) => "LF",
        nameof(PropertyCategory.SamajMandir) => "SM",
        nameof(PropertyCategory.Abhyasika) => "AB",
        nameof(PropertyCategory.Gaale256) => "256G",
        nameof(PropertyCategory.TP3_23) => "TP323",
        nameof(PropertyCategory.AdhikrutKhoke) => "AK",
        nameof(PropertyCategory.ItarBhadetatvavarilMalmatta) => "IT",
        _ => throw new InvalidOperationException("अवैध मालमत्ता प्रकार.")
    };

    public async Task<PagedResult<PropertyDto>> GetAllAsync(PropertyFilter filter)
    {
        var query = _db.Properties.AsNoTracking().Where(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filter.Category) && Enum.TryParse<PropertyCategory>(filter.Category, out var cat))
            query = query.Where(p => p.Category == cat);
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<PropertyStatus>(filter.Status, out var status))
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.Ward))
            query = query.Where(p => p.Ward == filter.Ward);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p =>
                p.PropertyCode.Contains(term) ||
                p.Name.Contains(term) ||
                (p.Address != null && p.Address.Contains(term)) ||
                (p.CurrentOccupant != null && p.CurrentOccupant.Contains(term)) ||
                (p.SurveyNumber != null && p.SurveyNumber.Contains(term)));
        }

        query = filter.SortBy switch
        {
            "Name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "AnnualDemand" => filter.SortDescending ? query.OrderByDescending(p => p.AnnualDemand) : query.OrderBy(p => p.AnnualDemand),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return new PagedResult<PropertyDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<PropertyDto?> GetByIdAsync(int id)
    {
        var p = await _db.Properties.AsNoTracking().Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return p is null ? null : ToDto(p);
    }

    public async Task<PropertyDto> CreateAsync(CreatePropertyDto dto, string createdBy)
    {
        var entity = new Property
        {
            Category = Enum.Parse<PropertyCategory>(dto.Category),
            PropertyCode = await GetNextCodeAsync(dto.Category),
            Name = dto.Name,
            Ward = dto.Ward,
            Zone = dto.Zone,
            Address = dto.Address,
            AreaSqFt = dto.AreaSqFt,
            MonthlyRent = dto.MonthlyRent,
            AnnualDemand = dto.AnnualDemand,
            SurveyNumber = dto.SurveyNumber,
            TpNumber = dto.TpNumber,
            Status = Enum.Parse<PropertyStatus>(dto.Status),
            CurrentOccupant = dto.CurrentOccupant,
            Shera = dto.Shera,
            CreatedBy = createdBy
        };
        _db.Properties.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdatePropertyDto dto, string updatedBy)
    {
        var entity = await _db.Properties.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;

        var before = new Property
        {
            Category = entity.Category, PropertyCode = entity.PropertyCode, Name = entity.Name,
            Ward = entity.Ward, Zone = entity.Zone, Address = entity.Address, AreaSqFt = entity.AreaSqFt,
            MonthlyRent = entity.MonthlyRent, AnnualDemand = entity.AnnualDemand, SurveyNumber = entity.SurveyNumber,
            TpNumber = entity.TpNumber, Status = entity.Status, CurrentOccupant = entity.CurrentOccupant, Shera = entity.Shera
        };

        entity.Category = Enum.Parse<PropertyCategory>(dto.Category);
        entity.PropertyCode = dto.PropertyCode;
        entity.Name = dto.Name;
        entity.Ward = dto.Ward;
        entity.Zone = dto.Zone;
        entity.Address = dto.Address;
        entity.AreaSqFt = dto.AreaSqFt;
        entity.MonthlyRent = dto.MonthlyRent;
        entity.AnnualDemand = dto.AnnualDemand;
        entity.SurveyNumber = dto.SurveyNumber;
        entity.TpNumber = dto.TpNumber;
        entity.Status = Enum.Parse<PropertyStatus>(dto.Status);
        entity.CurrentOccupant = dto.CurrentOccupant;
        entity.Shera = dto.Shera;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();


        return true;
    }

    public async Task<bool> DeleteAsync(int id, string deletedBy)
    {
        var entity = await _db.Properties.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity is null) return false;
        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
