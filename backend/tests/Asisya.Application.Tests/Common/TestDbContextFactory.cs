using Asisya.Infrastructure.Persistence;
using Asisya.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Tests.Common;

/// <summary>
/// Subclass of AsisyaDbContext overriding ExecuteInTransactionAsync for in-memory unit tests.
/// </summary>
public class TestAsisyaDbContext : AsisyaDbContext
{
    public TestAsisyaDbContext(DbContextOptions<AsisyaDbContext> options)
        : base(options)
    {
    }

    public override Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return operation(cancellationToken);
    }
}

/// <summary>
/// Factory creating isolated in-memory DbContext instances for unit testing.
/// </summary>
public static class TestDbContextFactory
{
    public static TestAsisyaDbContext Create(
        string? databaseName = null,
        AuditableEntitySaveChangesInterceptor? interceptor = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();

        var builder = new DbContextOptionsBuilder<AsisyaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName);

        if (interceptor != null)
        {
            builder.AddInterceptors(interceptor);
        }

        var context = new TestAsisyaDbContext(builder.Options);
        context.Database.EnsureCreated();

        return context;
    }

    public static void Destroy(TestAsisyaDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
