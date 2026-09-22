using Asisya.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure persistence and database services
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    service = "Asisya API",
    status = "Healthy",
    version = "1.0.0"
}));

app.Run();
