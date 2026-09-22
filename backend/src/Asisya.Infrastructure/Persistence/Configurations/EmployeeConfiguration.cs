using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Employee"/> entity.
/// Implements the hierarchical self-referential organizational tree via ReportsTo/Manager navigation.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.EmployeeId);

        builder.Property(e => e.EmployeeId)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .HasMaxLength(50);

        builder.Property(e => e.TitleOfCourtesy)
            .HasMaxLength(25);

        builder.Property(e => e.Address)
            .HasMaxLength(100);

        builder.Property(e => e.City)
            .HasMaxLength(50);

        builder.Property(e => e.Region)
            .HasMaxLength(50);

        builder.Property(e => e.PostalCode)
            .HasMaxLength(20);

        builder.Property(e => e.Country)
            .HasMaxLength(50);

        builder.Property(e => e.HomePhone)
            .HasMaxLength(30);

        builder.Property(e => e.Extension)
            .HasMaxLength(10);

        builder.Property(e => e.Photo)
            .HasColumnType("bytea");

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Self-referential hierarchical relationship: Manager -> Direct Reports
        builder.HasOne(e => e.Manager)
            .WithMany(m => m.DirectReports)
            .HasForeignKey(e => e.ReportsTo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReportsTo)
            .HasDatabaseName("IX_Employees_ReportsTo");
    }
}
