using Asisya.WebApi.Controllers;
using Asisya.WebApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Asisya.Application.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _jwtService = Substitute.For<IJwtService>();
        _logger = Substitute.For<ILogger<AuthController>>();
        _controller = new AuthController(_jwtService, _logger);
    }

    [Fact]
    public void Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        _jwtService.GenerateToken(Arg.Any<string>(), "admin@asisya.com", "Admin")
            .Returns("fake.jwt.token");

        var request = new LoginRequest("admin@asisya.com", "Admin123!");

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Token.Should().Be("fake.jwt.token");
        response.TokenType.Should().Be("Bearer");
        response.Role.Should().Be("Admin");
    }

    [Fact]
    public void Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest("hacker@bad.com", "WrongPassword");

        // Act
        var result = _controller.Login(request);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var problemDetails = unauthorizedResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Title.Should().Be("Invalid Credentials");
    }
}
