using Asisya.Application.Common.Interfaces;
using Asisya.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asisya.Infrastructure;

/// <summary>
/// Provides extension methods for registering Infrastructure layer services in the ASP.NET Core dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers database persistence, repositories, and external infrastructure services.
    /// </summary>
    /// <param name="services">The service collection descriptor.</param>
    /// <param name="configuration">Application configuration containing connection strings.</param>
    /// <returns>The modified service collection for method chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured in application settings.");

        services.AddDbContext<AsisyaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AsisyaDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        // Register IApplicationDbContext mapping to AsisyaDbContext
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AsisyaDbContext>());

        return services;
    }
}
