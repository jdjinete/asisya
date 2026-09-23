using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asisya.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="AuditLog"/> entity.
/// Defines table structure, property constraints, and B-tree indexes for fast audit query analysis.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.TableName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(a => a.UserId)
            .HasMaxLength(256);

        builder.Property(a => a.TimestampUtc)
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnType("text");

        builder.Property(a => a.NewValues)
            .HasColumnType("text");

        builder.Property(a => a.PrimaryKey)
            .HasMaxLength(256);

        // B-tree indexes for fast queries by entity, timestamp range, and user
        builder.HasIndex(a => a.TableName)
            .HasDatabaseName("IX_AuditLogs_TableName");

        builder.HasIndex(a => a.TimestampUtc)
            .HasDatabaseName("IX_AuditLogs_TimestampUtc");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");
    }
}
