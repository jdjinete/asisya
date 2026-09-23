using System.Text.Json;
using Asisya.Application.Common.Interfaces;
using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Asisya.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that automatically captures data mutations across tracked entities.
/// Generates <see cref="AuditLog"/> entries recording table name, mutation action (Insert, Update, Delete),
/// authenticated user identity, timestamp, and JSON serialization of OldValues and NewValues.
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditableEntitySaveChangesInterceptor"/> class.
    /// </summary>
    /// <param name="currentUserService">Service providing current user identity.</param>
    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AuditEntities(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AuditEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntities(DbContext context)
    {
        // Exclude AuditLog entities to prevent recursive auditing loops
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        (e.State == EntityState.Added ||
                         e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted))
            .ToList();

        if (entries.Count == 0) return;

        var responsibleUser = _currentUserService.UserEmail 
            ?? _currentUserService.UserId 
            ?? "System";

        var timestamp = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name;
            var action = entry.State switch
            {
                EntityState.Added => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => entry.State.ToString()
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var keyValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (!property.IsTemporary && property.CurrentValue != null)
                    {
                        keyValues[propertyName] = property.CurrentValue;
                    }
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            var auditLog = new AuditLog
            {
                TableName = tableName,
                Action = action,
                UserId = responsibleUser,
                TimestampUtc = timestamp,
                PrimaryKey = keyValues.Count > 0 ? JsonSerializer.Serialize(keyValues) : null,
                OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null
            };

            auditLogs.Add(auditLog);
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }
}
