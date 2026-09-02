using Microsoft.EntityFrameworkCore;
using SMC.Domain.Entities;

namespace SMC.Application.Interfaces;

/// <summary>Infrastructure मधील DbContext साठी Application layer मधून abstraction.</summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Property> Properties { get; }
    DbSet<Lease> Leases { get; }
    DbSet<RecoveryCase> RecoveryCases { get; }
    DbSet<SchemeApplication> SchemeApplications { get; }
    DbSet<AllocationProcess> AllocationProcesses { get; }
    DbSet<Calculation> Calculations { get; }
    DbSet<Document> Documents { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<DemandApplication> DemandApplications { get; }
    DbSet<DemandApplicationDocument> DemandApplicationDocuments { get; }
    DbSet<DemandApplicationWorkflow> DemandApplicationWorkflows { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
