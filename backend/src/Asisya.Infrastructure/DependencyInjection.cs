using Asisya.Application.Common.Interfaces;
using Asisya.Infrastructure.Persistence;
using Asisya.Infrastructure.Persistence.Interceptors;
using Asisya.Infrastructure.Services;
using MassTransit;
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
    /// Registers database persistence, repositories, auditing interceptor, user identity, and messaging infrastructure services.
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

        // Register HTTP context and current user resolution
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register EF Core Auditing Interceptor
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<AsisyaDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());

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

        // Configure MassTransit with RabbitMQ message broker & fault tolerance policies
        services.AddMassTransit(x =>
        {
            x.AddConsumer<Asisya.Application.Features.Products.Consumers.BulkCreateProductsConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var portStr = configuration["RabbitMq:Port"] ?? "5672";
                var port = ushort.TryParse(portStr, out var p) ? p : (ushort)5672;
                var username = configuration["RabbitMq:Username"] ?? "guest";
                var password = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, port, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Global retry policy for broker-level resilience: 3 retries, 2s interval
                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));

                // Dedicated consumer endpoint with explicit retry policy and automatic dead-letter queue (BulkCreateProducts_error)
                cfg.ReceiveEndpoint("BulkCreateProducts", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
                    e.ConfigureConsumer<Asisya.Application.Features.Products.Consumers.BulkCreateProductsConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
