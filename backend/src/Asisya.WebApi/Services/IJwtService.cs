namespace Asisya.WebApi.Services;

/// <summary>
/// Service contract for generating and validating JSON Web Tokens (JWT).
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT token containing standard and role-based claims.
    /// </summary>
    /// <param name="userId">The unique user subject identifier.</param>
    /// <param name="email">The user's corporate email address.</param>
    /// <param name="role">The assigned RBAC role (e.g. Admin, Operator).</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(string userId, string email, string role);
}
