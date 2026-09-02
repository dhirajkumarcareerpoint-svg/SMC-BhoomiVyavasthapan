using Microsoft.EntityFrameworkCore;
using SMC.Application.Interfaces;
using SMC.Domain.Entities;
using System.Globalization;

namespace SMC.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<RecoveryCase> RecoveryCases => Set<RecoveryCase>();
    public DbSet<SchemeApplication> SchemeApplications => Set<SchemeApplication>();
    public DbSet<AllocationProcess> AllocationProcesses => Set<AllocationProcess>();
    public DbSet<Calculation> Calculations => Set<Calculation>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DemandApplication> DemandApplications => Set<DemandApplication>();
    public DbSet<DemandApplicationDocument> DemandApplicationDocuments => Set<DemandApplicationDocument>();
    public DbSet<DemandApplicationWorkflow> DemandApplicationWorkflows => Set<DemandApplicationWorkflow>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = ChangeTracker.Entries()
            .Where(entry => entry.Entity is not AuditLog && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CreateAuditChanges)
            .SelectMany(change => change)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);
        if (changes.Count == 0 || !_currentUser.UserId.HasValue) return result;

        foreach (var change in changes)
        {
            var entry = change.Entry;
            AuditLogs.Add(new AuditLog
            {
                UserId = _currentUser.UserId ?? 0,
                UserName = _currentUser.UserName ?? "System",
                Action = change.Action,
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = Convert.ToInt32(entry.Property("Id").CurrentValue, CultureInfo.InvariantCulture),
                FieldName = change.FieldName,
                OldValue = change.OldValue,
                NewValue = change.NewValue,
                Timestamp = DateTime.UtcNow,
                IpAddress = _currentUser.IpAddress
            });
        }

        await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    private static IEnumerable<AuditChange> CreateAuditChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var action = entry.State == EntityState.Added ? "Create"
            : entry.State == EntityState.Deleted || IsSoftDeleted(entry) ? "Delete" : "Update";
        var properties = entry.Properties.Where(property => IsAuditable(property.Metadata.Name));

        if (action is "Create" or "Delete")
        {
            var value = GetSummary(entry, action == "Create" ? EntityState.Added : EntityState.Deleted);
            yield return new AuditChange(entry, action, null, action == "Create" ? null : value, action == "Create" ? value : null);
            yield break;
        }

        foreach (var property in properties.Where(property => property.IsModified
            && !Equals(property.OriginalValue, property.CurrentValue)))
        {
            yield return new AuditChange(entry, action, property.Metadata.Name,
                FormatValue(property.OriginalValue), FormatValue(property.CurrentValue));
        }
    }

    private static bool IsSoftDeleted(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        entry.State == EntityState.Modified
        && entry.Properties.Any(property => property.Metadata.Name == "IsDeleted" && property.CurrentValue is true);

    private static bool IsAuditable(string propertyName) => propertyName is not (
        "Id" or "CreatedAt" or "CreatedBy" or "UpdatedAt" or "UpdatedBy" or
        "IsDeleted" or "DeletedAt" or "DeletedBy" or "PasswordHash" or "LastLoginAt");

    private static string? GetSummary(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, EntityState state)
    {
        var preferred = new[] { "PropertyCode", "Username", "DeedNumber", "NoticeNumber", "Name", "FullName" };
        var values = preferred
            .Where(name => entry.Metadata.FindProperty(name) is not null)
            .Select(name => FormatValue(entry.Property(name).CurrentValue ?? entry.Property(name).OriginalValue))
            .Where(value => !string.IsNullOrWhiteSpace(value));
        var summary = string.Join(" - ", values);
        return string.IsNullOrWhiteSpace(summary) ? entry.Metadata.ClrType.Name : summary;
    }

    private static string? FormatValue(object? value) => value switch
    {
        null => null,
        DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private sealed record AuditChange(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry,
        string Action,
        string? FieldName,
        string? OldValue,
        string? NewValue);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter: soft-deleted records default दिसू नयेत.
        builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Lease>().HasQueryFilter(l => !l.IsDeleted);
        builder.Entity<RecoveryCase>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<SchemeApplication>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<AllocationProcess>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<Calculation>().HasQueryFilter(c => !c.IsDeleted);
        builder.Entity<Document>().HasQueryFilter(d => !d.IsDeleted);

        base.OnModelCreating(builder);
    }
}
