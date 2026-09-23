using Asisya.Application.Common.Models;
using MediatR;

namespace Asisya.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Query to retrieve a filtered and server-side paginated list of audit logs in descending chronological order.
/// Satisfies the operational and compliance requirements for GET /AuditLogs.
/// </summary>
public record GetAuditLogsQuery : IRequest<PaginatedList<AuditLogDto>>
{
    /// <summary>
    /// Current 1-based page index (default: 1).
    /// </summary>
    public int PageIndex { get; init; } = 1;

    /// <summary>
    /// Quantity of items per page (default: 10, max: 100).
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional filter by table name (e.g., Products, Categories).
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Optional filter by action type (e.g., Insert, Update, Delete).
    /// </summary>
    public string? Action { get; init; }
}
