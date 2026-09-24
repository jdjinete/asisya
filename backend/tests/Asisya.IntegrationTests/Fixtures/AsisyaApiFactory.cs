using Asisya.Application.Common.Interfaces;
using Asisya.Infrastructure.Persistence;
using Asisya.Infrastructure.Persistence.Interceptors;
using Asisya.WebApi.Services;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Asisya.IntegrationTests.Fixtures;

/// <summary>
/// WebApplicationFactory fixture that spins up a real ephemeral PostgreSQL container using Testcontainers.
/// Executes EF Core migrations against the ephemeral database, exposes authenticated HTTP clients,
/// and tears down the container automatically when the test run concludes.
/// </summary>
public class AsisyaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("asisya_test_db")
        .WithUsername("asisya_test_user")
        .WithPassword("AsisyaTestPass2026!")
        .WithCleanUp(true)
        .Build();

    /// <summary>
    /// Starts the PostgreSQL testcontainer and executes EF Core migrations.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Run migrations explicitly to ensure clean, isolated schema
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AsisyaDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>
    /// Stops and destroys the ephemeral PostgreSQL container.
    /// </summary>
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Jwt:Key"] = "AsisyaEnterpriseSuperSecretEncryptionKeyForJwt2026!MustBe256BitsMinimum",
                ["Jwt:Issuer"] = "AsisyaAuthServer",
                ["Jwt:Audience"] = "AsisyaApp"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove existing EF Core DbContext registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AsisyaDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            d.ServiceType == typeof(AsisyaDbContext) ||
                            d.ServiceType == typeof(IApplicationDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Register AsisyaDbContext with dynamic Testcontainers PostgreSQL connection string
            services.AddDbContext<AsisyaDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
                options.AddInterceptors(interceptor);

                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AsisyaDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });
            });

            // Re-bind IApplicationDbContext to the reconfigured AsisyaDbContext
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AsisyaDbContext>());

            // Replace external RabbitMQ transport with MassTransit InMemory test harness for test isolation
            services.AddMassTransitTestHarness();
        });
    }

    /// <summary>
    /// Generates a valid JWT bearer token with Administrator role.
    /// </summary>
    public string GenerateAdminToken()
    {
        using var scope = Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        return jwtService.GenerateToken("admin-test-01", "admin.tester@asisya.com", "Administrator");
    }

    /// <summary>
    /// Creates an HttpClient pre-configured with a valid JWT Authorization header.
    /// </summary>
    public HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GenerateAdminToken());
        return client;
    }
}
