using Asisya.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Asisya.WebApi.Controllers;

/// <summary>
/// Data transfer object for authentication requests.
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Data transfer object containing the issued JWT authentication token.
/// </summary>
public record LoginResponse(string Token, string TokenType, int ExpiresInSeconds, string Email, string Role);

/// <summary>
/// Authentication endpoint for issuing stateless JWT bearer tokens.
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a client and generates a signed JWT token.
    /// </summary>
    /// <param name="request">Login credentials (e.g. admin@asisya.com / Admin123!).</param>
    /// <returns>A signed JWT Bearer token.</returns>
    [HttpPost("Login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Default enterprise admin credentials for evaluation
        if ((request.Email == "admin@asisya.com" && request.Password == "Admin123!") ||
            (request.Email == "operator@asisya.com" && request.Password == "Operator123!"))
        {
            var role = request.Email.StartsWith("admin") ? "Admin" : "Operator";
            var token = _jwtService.GenerateToken("usr-001", request.Email, role);

            _logger.LogInformation("User {Email} authenticated successfully with role {Role}", request.Email, role);

            return Ok(new LoginResponse(
                Token: token,
                TokenType: "Bearer",
                ExpiresInSeconds: 3600,
                Email: request.Email,
                Role: role
            ));
        }

        _logger.LogWarning("Failed authentication attempt for email: {Email}", request.Email);

        return Unauthorized(new ProblemDetails
        {
            Title = "Invalid Credentials",
            Detail = "The email or password provided is incorrect.",
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        });
    }
}
