using System.Security.Claims;
using Asisya.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Asisya.Infrastructure.Services;

/// <summary>
/// Service extracting the authenticated user identity and claims from the active HTTP request context.
/// Provides safe fallbacks when executing within background workers (e.g. MassTransit consumers)
/// where <see cref="HttpContext"/> is not present.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor to the current HTTP context.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
        ?? (_httpContextAccessor.HttpContext == null ? "System / BackgroundWorker" : "Anonymous");

    /// <inheritdoc />
    public string? UserEmail =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
        ?? (_httpContextAccessor.HttpContext == null ? "system@asisya.com" : null);

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
