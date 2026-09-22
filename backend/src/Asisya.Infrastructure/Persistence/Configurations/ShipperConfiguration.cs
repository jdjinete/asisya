using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Shipper"/> entity.
/// </summary>
public class ShipperConfiguration : IEntityTypeConfiguration<Shipper>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Shipper> builder)
    {
        builder.ToTable("Shippers");

        builder.HasKey(s => s.ShipperId);

        builder.Property(s => s.ShipperId)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .HasMaxLength(30);
    }
}
