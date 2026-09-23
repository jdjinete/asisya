using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Asisya.Application.Tests.HealthChecks;

/// <summary>
/// Unit tests verifying the registration and execution pipeline of ASP.NET Core Health Checks.
/// </summary>
public class HealthCheckRegistrationTests
{
    [Fact]
    public void AddHealthChecks_ShouldRegisterHealthCheckService_InDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHealthChecks()
            .AddAsyncCheck("TestCheck", () => Task.FromResult(HealthCheckResult.Healthy("All systems normal")));

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetService<HealthCheckService>();

        // Assert
        healthCheckService.Should().NotBeNull();
    }

    [Fact]
    public async Task HealthCheckService_ShouldExecuteRegisteredChecks_AndReturnHealthyStatus()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks()
            .AddAsyncCheck("SampleComponent", () => Task.FromResult(HealthCheckResult.Healthy("Operational")));

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetRequiredService<HealthCheckService>();

        // Act
        var report = await healthCheckService.CheckHealthAsync();

        // Assert
        report.Status.Should().Be(HealthStatus.Healthy);
        report.Entries.Should().ContainKey("SampleComponent");
        report.Entries["SampleComponent"].Status.Should().Be(HealthStatus.Healthy);
        report.Entries["SampleComponent"].Description.Should().Be("Operational");
    }
}
