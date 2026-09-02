using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SMC.Application.Interfaces;
using SMC.Infrastructure.Persistence;
using SMC.Infrastructure.Services;

namespace SMC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.Configure<SmsOptions>(config.GetSection("Integrations:Sms"));
        services.Configure<RazorpayOptions>(config.GetSection("Razorpay"));
        services.AddHttpClient("AclSms", client => client.Timeout = TimeSpan.FromSeconds(20));
        services.AddHttpClient("Razorpay", client => client.Timeout = TimeSpan.FromSeconds(20));
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IRazorpayGateway, RazorpayGateway>();

        return services;
    }
}
