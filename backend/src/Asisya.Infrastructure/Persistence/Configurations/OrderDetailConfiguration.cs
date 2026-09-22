using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="OrderDetail"/> entity.
/// Defines the composite primary key (OrderId, ProductId) and referential constraints
/// ensuring price auditability and preventing deletion of cataloged products active in historical orders.
/// </summary>
public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("OrderDetails");

        // Composite Primary Key (OrderId, ProductId)
        builder.HasKey(od => new { od.OrderId, od.ProductId });

        builder.Property(od => od.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(od => od.Quantity)
            .HasDefaultValue((short)1)
            .IsRequired();

        builder.Property(od => od.Discount)
            .HasColumnType("real")
            .HasDefaultValue(0.0f)
            .IsRequired();

        // Foreign key to Order: If an order is purged, line items are cascaded
        builder.HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to Product: Restrict deletion of products present in historical orders
        builder.HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(od => od.ProductId)
            .HasDatabaseName("IX_OrderDetails_ProductId");
    }
}
