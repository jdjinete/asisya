namespace Asisya.Domain.Entities;

/// <summary>
/// Domain entity representing an audit log entry for changes made to database records.
/// Tracks data mutations across entities, capturing change deltas (OldValues and NewValues in JSON format),
/// UTC timestamp, mutation action type, and responsible user identity for compliance and governance.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Database table name where the modification occurred.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Action performed on the record: "Insert", "Update", or "Delete".
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Responsible user identifier or email who executed the mutation.
    /// Extracted from JWT claims or background worker identity context.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// UTC timestamp when the database mutation was committed.
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON representation of previous property values before the mutation (for Update and Delete).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON representation of new property values after the mutation (for Insert and Update).
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Primary key value(s) of the affected entity.
    /// </summary>
    public string? PrimaryKey { get; set; }
}
