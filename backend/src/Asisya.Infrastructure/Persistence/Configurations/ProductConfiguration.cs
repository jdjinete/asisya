using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Product"/> entity.
/// Enforces referential integrity with Category and Supplier, along with performance indexes
/// tailored for high-throughput pagination, filtering, and search queries.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductId)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.QuantityPerUnit)
            .HasMaxLength(50);

        builder.Property(p => p.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.UnitsInStock)
            .HasDefaultValue((short)0);

        builder.Property(p => p.UnitsOnOrder)
            .HasDefaultValue((short)0);

        builder.Property(p => p.ReorderLevel)
            .HasDefaultValue((short)0);

        builder.Property(p => p.Discontinued)
            .IsRequired()
            .HasDefaultValue(false);

        // 1:N Relationship Category -> Products
        // Critical: ON DELETE RESTRICT prevents accidental orphan products or cascading catalog corruption
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship Supplier -> Products
        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Performance Optimization: B-Tree Index on ProductName for fast prefix/exact matches in GET /Products
        builder.HasIndex(p => p.ProductName)
            .HasDatabaseName("IX_Products_ProductName");

        // Performance Optimization: B-Tree Index on CategoryId for fast foreign-key joins and filtering
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        // Performance Optimization: Composite index for active catalog filtering (Discontinued + CategoryId)
        builder.HasIndex(p => new { p.Discontinued, p.CategoryId })
            .HasDatabaseName("IX_Products_Discontinued_CategoryId");
    }
}
