using System.Text;
using Asisya.Infrastructure.Persistence;
using Asisya.WebApi.Middlewares;
using Asisya.WebApi.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Asisya.WebApi;

/// <summary>
/// Provides extension methods for registering presentation layer services, HTTP middleware,
/// authentication, documentation, and observability in the ASP.NET Core host container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all presentation and API boundary services in the DI container.
    /// </summary>
    /// <param name="services">The service collection descriptor.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The modified service collection for method chaining.</returns>
    public static IServiceCollection AddWebApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddCorsPolicy();
        services.AddSwaggerDocumentation();
        services.AddAuthServices(configuration);
        services.AddHealthCheckServices(configuration);

        return services;
    }

    /// <summary>
    /// Configures Cross-Origin Resource Sharing (CORS) policies.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI specification generation with JWT Bearer security schemes.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ASISYA Commerce & Catalog Web API",
                Version = "v1",
                Description = "Enterprise Clean Architecture solution in .NET 8 for category management, product catalog search, and high-performance streaming batch ingestion."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token: Bearer {your token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer authentication, authorization policies, and token generator service.
    /// </summary>
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IJwtService, JwtService>();

        var jwtKey = configuration["Jwt:Key"] 
            ?? "AsisyaEnterpriseSuperSecretEncryptionKeyForJwt2026!MustBe256BitsMinimum";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "AsisyaAuthServer";
        var jwtAudience = configuration["Jwt:Audience"] ?? "AsisyaApp";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Configures infrastructure health checks (PostgreSQL, RabbitMQ) and UI dashboard.
    /// </summary>
    public static IServiceCollection AddHealthCheckServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var pgConnectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=asisyadb;Username=asisya_user;Password=AsisyaPass2026!;";

        var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
        var rabbitPortStr = configuration["RabbitMq:Port"] ?? "5672";
        var rabbitPort = int.TryParse(rabbitPortStr, out var rp) ? rp : 5672;
        var rabbitUser = configuration["RabbitMq:Username"] ?? "guest";
        var rabbitPass = configuration["RabbitMq:Password"] ?? "guest";
        var rabbitConnectionString = $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:{rabbitPort}/";

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString: pgConnectionString,
                name: "PostgreSQL Database",
                tags: new[] { "db", "sql", "readiness" })
            .AddRabbitMQ(
                async sp =>
                {
                    var factory = new RabbitMQ.Client.ConnectionFactory
                    {
                        Uri = new Uri(rabbitConnectionString)
                    };
                    return await factory.CreateConnectionAsync();
                },
                name: "RabbitMQ Message Broker",
                tags: new[] { "messaging", "broker", "readiness" });

        var healthEndpointUri = configuration["HealthChecksUI:Endpoint"] 
            ?? "http://127.0.0.1:8080/health";

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(10);
            setup.MaximumHistoryEntriesPerEndpoint(50);
            setup.AddHealthCheckEndpoint("ASISYA Infrastructure Health", healthEndpointUri);
        })
        .AddInMemoryStorage();

        return services;
    }

    /// <summary>
    /// Applies pending Entity Framework Core migrations automatically during host bootstrap.
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AsisyaDbContext>();
            if (db.Database.IsRelational())
            {
                logger.LogInformation("Applying pending PostgreSQL migrations...");
                await db.Database.MigrateAsync();
                logger.LogInformation("PostgreSQL migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not apply database migrations on startup. Verify database readiness.");
        }
    }

    /// <summary>
    /// Configures the HTTP request pipeline, middleware, and route mappings.
    /// </summary>
    public static WebApplication UseWebApiPipeline(this WebApplication app)
    {
        // Global exception handling middleware
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        // Enable Swagger documentation and UI at root URL
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASISYA API v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        // Observability health checks endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });

        app.MapControllers();

        return app;
    }
}
