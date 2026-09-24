using Asisya.Application;
using Asisya.Infrastructure;
using Asisya.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Register Clean Architecture layers
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddWebApiServices(builder.Configuration);

var app = builder.Build();

// Apply automated database migrations on startup
await app.ApplyDatabaseMigrationsAsync();

// Configure HTTP request pipeline and endpoints
app.UseWebApiPipeline();

app.Run();

// Required for WebApplicationFactory integration testing
public partial class Program { }
