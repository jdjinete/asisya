using Asisya.Application.Common.Models;
using Asisya.Application.Features.AuditLogs.Queries.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asisya.WebApi.Controllers;

/// <summary>
/// Controller providing compliance and audit trail visibility.
/// Allows authenticated operators to inspect database mutation logs, change deltas, and user activity.
/// </summary>
[ApiController]
[Authorize]
[Route("[controller]")]
[Route("Audit")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuditLogsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogsController"/> class.
    /// </summary>
    public AuditLogsController(IMediator mediator, ILogger<AuditLogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of system audit logs ordered chronologically descending.
    /// Supports optional filtering by table name and action type (Insert, Update, Delete).
    /// </summary>
    /// <param name="query">Pagination parameters (PageIndex, PageSize) and optional filters.</param>
    /// <returns>A paginated list of audit records including change payloads.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsQuery query)
    {
        _logger.LogInformation("Retrieving audit logs: Page {Page}, PageSize {PageSize}, Table '{Table}', Action '{Action}'",
            query.PageIndex, query.PageSize, query.TableName, query.Action);

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
