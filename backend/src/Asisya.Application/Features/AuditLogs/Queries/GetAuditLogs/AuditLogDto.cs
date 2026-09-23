namespace Asisya.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Data transfer object representing an audit log entry.
/// Exposes historical database mutation details for governance, tracking, and compliance inspection.
/// </summary>
/// <param name="Id">Unique identifier of the audit log record.</param>
/// <param name="TableName">Database table name where the change occurred.</param>
/// <param name="Action">Action performed on the record (Insert, Update, Delete).</param>
/// <param name="UserId">User email or sub claim identifying who performed the action.</param>
/// <param name="TimestampUtc">UTC timestamp when the change was recorded.</param>
/// <param name="OldValues">JSON payload representing the state before the mutation (for Update/Delete).</param>
/// <param name="NewValues">JSON payload representing the state after the mutation (for Insert/Update).</param>
/// <param name="PrimaryKey">JSON representation of the primary key of the modified entity.</param>
public record AuditLogDto(
    long Id,
    string TableName,
    string Action,
    string? UserId,
    DateTime TimestampUtc,
    string? OldValues,
    string? NewValues,
    string? PrimaryKey);
