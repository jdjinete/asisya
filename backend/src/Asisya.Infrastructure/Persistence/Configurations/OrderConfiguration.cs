using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Order"/> entity.
/// Maps relations with Customer, Employee, and Shipper logistics.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.OrderId);

        builder.Property(o => o.OrderId)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerId)
            .HasMaxLength(5);

        builder.Property(o => o.Freight)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.ShipName)
            .HasMaxLength(100);

        builder.Property(o => o.ShipAddress)
            .HasMaxLength(100);

        builder.Property(o => o.ShipCity)
            .HasMaxLength(50);

        builder.Property(o => o.ShipRegion)
            .HasMaxLength(50);

        builder.Property(o => o.ShipPostalCode)
            .HasMaxLength(20);

        builder.Property(o => o.ShipCountry)
            .HasMaxLength(50);

        // Relationships with Customer, Employee, Shipper
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Employee)
            .WithMany(e => e.Orders)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Shipper)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.ShipVia)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("IX_Orders_CustomerId");

        builder.HasIndex(o => o.OrderDate)
            .HasDatabaseName("IX_Orders_OrderDate");
    }
}
