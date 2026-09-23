using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Category"/> entity.
/// Defines table mapping, constraints, and indices for product categorization.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryId)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Picture)
            .HasColumnType("bytea");

        // B-tree index on CategoryName to optimize alphabetical listing and lookup by name
        builder.HasIndex(c => c.CategoryName)
            .HasDatabaseName("IX_Categories_CategoryName");
    }
}
