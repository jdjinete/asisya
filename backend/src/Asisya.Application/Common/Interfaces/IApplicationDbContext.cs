using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Asisya.Application.Common.Interfaces;

/// <summary>
/// Abstraction for database persistence operations decoupled from the Infrastructure layer.
/// Exposes Entity Framework Core DbSets and transaction management.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Categories catalog set.
    /// </summary>
    DbSet<Category> Categories { get; }

    /// <summary>
    /// Products catalog set.
    /// </summary>
    DbSet<Product> Products { get; }

    /// <summary>
    /// Suppliers vendor set.
    /// </summary>
    DbSet<Supplier> Suppliers { get; }

    /// <summary>
    /// Customers client set.
    /// </summary>
    DbSet<Customer> Customers { get; }

    /// <summary>
    /// Employees staff set.
    /// </summary>
    DbSet<Employee> Employees { get; }

    /// <summary>
    /// Shippers logistics set.
    /// </summary>
    DbSet<Shipper> Shippers { get; }

    /// <summary>
    /// Commercial orders set.
    /// </summary>
    DbSet<Order> Orders { get; }

    /// <summary>
    /// Order line details set.
    /// </summary>
    DbSet<OrderDetail> OrderDetails { get; }

    /// <summary>
    /// Asynchronously saves pending tracked entity changes to the underlying database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction asynchronously.
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the EF Core change tracker to prevent memory pressure (LOH) and O(N^2) change tracking overhead during bulk batch inserts.
    /// </summary>
    void ClearChangeTracker();
}
