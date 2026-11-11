using SampleRack.Application.Interfaces;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Infrastructure.Identity.Repositories;
using SampleRack.Infrastructure.Identity.Services;
using SampleRack.Infrastructure.Persistence;
using SampleRack.Infrastructure.Persistence.Interceptors;
using SampleRack.Infrastructure.SampleTracking.Repositories;
using SampleRack.Infrastructure.Services;
using SampleRack.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SampleRack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddServices(services);
        AddRepositories(services);
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditTrailInterceptor>();

        services.AddDbContext<SampleRackDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditTrailInterceptor>();
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", "shared");
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
            options.AddInterceptors(auditInterceptor);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SampleRackDbContext>());
        services.AddScoped<IExecutionStrategyFactory, EfExecutionStrategyFactory>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<IClockService, ClockService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, FileStorage.LocalFileStorageService>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        // Identity
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Sample tracking
        services.AddScoped<IRackLocationRepository, RackLocationRepository>();
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddScoped<ISampleTrackingSettingsRepository, SampleTrackingSettingsRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMasterDataRepository, MasterDataRepository>();
    }
}
