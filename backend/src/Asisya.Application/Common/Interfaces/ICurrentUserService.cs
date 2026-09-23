namespace Asisya.Application.Common.Interfaces;

/// <summary>
/// Cross-cutting abstraction to access the current authenticated user's identity
/// from the active HTTP request context or background execution context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier (subject) of the current user.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the email address of the current user.
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Indicates whether the current execution context has an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }
}
