using Asisya.Domain.Entities;
using Asisya.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;

namespace Asisya.Application.Tests.Common;

/// <summary>
/// Subclass of AsisyaDbContext overriding BeginTransactionAsync to provide a stub transaction for InMemory tests.
/// </summary>
public class TestAsisyaDbContext : AsisyaDbContext
{
    public TestAsisyaDbContext(DbContextOptions<AsisyaDbContext> options)
        : base(options)
    {
    }

    public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = Substitute.For<IDbContextTransaction>();
        return Task.FromResult(transaction);
    }
}

/// <summary>
/// Factory creating isolated in-memory DbContext instances for unit testing.
/// </summary>
public static class TestDbContextFactory
{
    public static TestAsisyaDbContext Create(string? databaseName = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AsisyaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new TestAsisyaDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static void Destroy(TestAsisyaDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
