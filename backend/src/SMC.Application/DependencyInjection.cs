using Microsoft.Extensions.DependencyInjection;
using SMC.Application.Interfaces;
using SMC.Application.Services;

namespace SMC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<ILeaseService, LeaseService>();
        services.AddScoped<IRecoveryCaseService, RecoveryCaseService>();
        services.AddScoped<ISchemeApplicationService, SchemeApplicationService>();
        services.AddScoped<IAllocationProcessService, AllocationProcessService>();
        services.AddScoped<ICalculationService, CalculationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDemandApplicationService, DemandApplicationService>();
        services.AddScoped<IDemandWorkflowService, DemandWorkflowService>();
            return services;
    }
}
