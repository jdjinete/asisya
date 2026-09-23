using Asisya.Application.Common.Interfaces;
using Asisya.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// CQRS handler for GetAuditLogsQuery retrieving paginated audit entries in descending chronological order.
/// </summary>
public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuditLogsQueryHandler"/> class.
    /// </summary>
    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PaginatedList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.TableName))
        {
            var tableFilter = request.TableName.Trim().ToLower();
            query = query.Where(a => a.TableName.ToLower().Contains(tableFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var actionFilter = request.Action.Trim().ToLower();
            query = query.Where(a => a.Action.ToLower() == actionFilter);
        }

        query = query.OrderByDescending(a => a.TimestampUtc);

        var projectedQuery = query.Select(a => new AuditLogDto(
            a.Id,
            a.TableName,
            a.Action,
            a.UserId,
            a.TimestampUtc,
            a.OldValues,
            a.NewValues,
            a.PrimaryKey
        ));

        return await PaginatedList<AuditLogDto>.CreateAsync(projectedQuery, pageIndex, pageSize, cancellationToken);
    }
}
