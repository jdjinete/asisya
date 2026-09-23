using Asisya.Application.Common.Interfaces;
using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the ASISYA enterprise application.
/// Manages entity configurations, relational constraints, and connection lifecycle to PostgreSQL.
/// Implements <see cref="IApplicationDbContext"/> for application layer decoupling.
/// </summary>
public class AsisyaDbContext : DbContext, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsisyaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public AsisyaDbContext(DbContextOptions<AsisyaDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public DbSet<Category> Categories => Set<Category>();

    /// <inheritdoc />
    public DbSet<Product> Products => Set<Product>();

    /// <inheritdoc />
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <inheritdoc />
    public DbSet<Customer> Customers => Set<Customer>();

    /// <inheritdoc />
    public DbSet<Employee> Employees => Set<Employee>();

    /// <inheritdoc />
    public DbSet<Shipper> Shippers => Set<Shipper>();

    /// <inheritdoc />
    public DbSet<Order> Orders => Set<Order>();

    /// <inheritdoc />
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    /// <inheritdoc />
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public virtual void ClearChangeTracker()
    {
        ChangeTracker.Clear();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dynamically discover and apply all IEntityTypeConfiguration implementations in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AsisyaDbContext).Assembly);
    }
}
