using System.Text;
using Asisya.Application;
using Asisya.Infrastructure;
using Asisya.Infrastructure.Persistence;
using Asisya.WebApi.Middlewares;
using Asisya.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application and Infrastructure layers (Clean Architecture)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Add Web API Services
builder.Services.AddControllers();
builder.Services.AddScoped<IJwtService, JwtService>();

// 3. Configure JWT Authentication & Authorization
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? "AsisyaEnterpriseSuperSecretEncryptionKeyForJwt2026!MustBe256BitsMinimum";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AsisyaAuthServer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AsisyaApp";

builder.Services.AddAuthentication(options =>
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

builder.Services.AddAuthorization();

// 4. Configure CORS for client access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Configure Swagger OpenAPI Documentation with JWT Bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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

// 6. Configure Health Checks & Observability
var pgConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=asisyadb;Username=asisya_user;Password=AsisyaPass2026!;";

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitPortStr = builder.Configuration["RabbitMq:Port"] ?? "5672";
var rabbitPort = int.TryParse(rabbitPortStr, out var rp) ? rp : 5672;
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";
var rabbitConnectionString = $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:{rabbitPort}/";

builder.Services.AddHealthChecks()
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

var healthEndpointUri = builder.Configuration["HealthChecksUI:Endpoint"] 
    ?? "http://127.0.0.1:8080/health";

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(10);
    setup.MaximumHistoryEntriesPerEndpoint(50);
    setup.AddHealthCheckEndpoint("ASISYA Infrastructure Health", healthEndpointUri);
})
.AddInMemoryStorage();

var app = builder.Build();

// 6. Automatic Database Migration upon container/app startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AsisyaDbContext>();
        if (db.Database.IsRelational())
        {
            logger.LogInformation("Applying pending PostgreSQL migrations...");
            db.Database.Migrate();
            logger.LogInformation("PostgreSQL migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not apply database migrations on startup. Verify database readiness.");
    }
}

// 7. Configure HTTP Request Pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Enable Swagger in all environments for evaluation convenience
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASISYA API v1");
    c.RoutePrefix = string.Empty; // Swagger UI served at root /
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// 8. Health Check and Observability Endpoints
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

app.Run();

// Required for WebApplicationFactory integration testing
public partial class Program { }
