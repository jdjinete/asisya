using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the ASISYA enterprise application.
/// Manages entity configurations, relational constraints, and connection lifecycle to PostgreSQL.
/// </summary>
public class AsisyaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsisyaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public AsisyaDbContext(DbContextOptions<AsisyaDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the Categories database set.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Gets the Products database set.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets the Suppliers database set.
    /// </summary>
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <summary>
    /// Gets the Customers database set.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets the Employees database set.
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>
    /// Gets the Shippers database set.
    /// </summary>
    public DbSet<Shipper> Shippers => Set<Shipper>();

    /// <summary>
    /// Gets the Orders database set.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Gets the OrderDetails database set.
    /// </summary>
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dynamically discover and apply all IEntityTypeConfiguration implementations in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AsisyaDbContext).Assembly);
    }
}
