using Microsoft.EntityFrameworkCore;
using SMC.Application.DTOs;
using SMC.Application.Interfaces;
using SMC.Domain.Enums;

namespace SMC.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;

    public DashboardService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var properties = _db.Properties.AsNoTracking().Where(p => !p.IsDeleted);
        var propertySummary = await properties
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalProperties = g.Count(),
                TotalShops = g.Count(p => p.Category == PropertyCategory.MajorGaale
                    || p.Category == PropertyCategory.MiniGaale || p.Category == PropertyCategory.Gaale256),
                Vacant = g.Count(p => p.Status == PropertyStatus.Rikamy),
                Leased = g.Count(p => p.Status == PropertyStatus.Bhadyane),
                Sealed = g.Count(p => p.Status == PropertyStatus.Seal),
                AnnualDemand = g.Sum(p => (decimal?)p.AnnualDemand) ?? 0
            })
            .FirstOrDefaultAsync();

        var recoverySummary = await _db.RecoveryCases.AsNoTracking().Where(r => !r.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalCollection = g.Sum(r => (decimal?)r.RecoveredAmount) ?? 0,
                TotalOutstanding = g.Where(r => r.Stage != RecoveryStage.Band)
                    .Sum(r => (decimal?)r.OutstandingAmount) ?? 0,
                PendingCases = g.Count(r => r.Stage != RecoveryStage.Band)
            })
            .FirstOrDefaultAsync();

        var categoryBreakdown = await properties
            .GroupBy(p => p.Category)
            .Select(g => new CategoryCountDto
            {
                Category = g.Key.ToString(),
                Count = g.Count(),
                AnnualDemand = g.Sum(p => p.AnnualDemand)
            }).ToListAsync();

        var monthlyGroups = await _db.RecoveryCases.AsNoTracking().Where(r => !r.IsDeleted && r.RecoveryDate != null
                && r.RecoveryDate >= DateTime.UtcNow.AddMonths(-11))
            .GroupBy(r => new { r.RecoveryDate!.Value.Year, r.RecoveryDate!.Value.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Amount = g.Sum(r => r.RecoveredAmount)
            }).OrderBy(m => m.Year).ThenBy(m => m.Month).ToListAsync();

        var monthly = monthlyGroups.Select(m => new MonthlyCollectionDto
        {
            Month = $"{m.Year}-{m.Month:00}",
            Amount = m.Amount
        }).ToList();

        return new DashboardSummaryDto
        {
            TotalProperties = propertySummary?.TotalProperties ?? 0,
            TotalShops = propertySummary?.TotalShops ?? 0,
            VacantProperties = propertySummary?.Vacant ?? 0,
            LeasedProperties = propertySummary?.Leased ?? 0,
            SealedProperties = propertySummary?.Sealed ?? 0,
            AnnualDemand = propertySummary?.AnnualDemand ?? 0,
            TotalCollection = recoverySummary?.TotalCollection ?? 0,
            TotalOutstanding = recoverySummary?.TotalOutstanding ?? 0,
            PendingRecoveryCases = recoverySummary?.PendingCases ?? 0,
            CategoryBreakdown = categoryBreakdown,
            MonthlyCollection = monthly
        };
    }
}
