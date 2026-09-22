using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Supplier"/> entity.
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.SupplierId);

        builder.Property(s => s.SupplierId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ContactName)
            .HasMaxLength(60);

        builder.Property(s => s.ContactTitle)
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(100);

        builder.Property(s => s.City)
            .HasMaxLength(50);

        builder.Property(s => s.Region)
            .HasMaxLength(50);

        builder.Property(s => s.PostalCode)
            .HasMaxLength(20);

        builder.Property(s => s.Country)
            .HasMaxLength(50);

        builder.Property(s => s.Phone)
            .HasMaxLength(30);

        builder.Property(s => s.Fax)
            .HasMaxLength(30);

        builder.Property(s => s.HomePage)
            .HasMaxLength(255);
    }
}
