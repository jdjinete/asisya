using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Customer"/> entity.
/// Uses fixed-length 5-character string primary key according to legacy standard.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.CustomerId)
            .HasMaxLength(5)
            .IsFixedLength()
            .IsRequired();

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ContactName)
            .HasMaxLength(60);

        builder.Property(c => c.ContactTitle)
            .HasMaxLength(50);

        builder.Property(c => c.Address)
            .HasMaxLength(100);

        builder.Property(c => c.City)
            .HasMaxLength(50);

        builder.Property(c => c.Region)
            .HasMaxLength(50);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.Country)
            .HasMaxLength(50);

        builder.Property(c => c.Phone)
            .HasMaxLength(30);

        builder.Property(c => c.Fax)
            .HasMaxLength(30);
    }
}
