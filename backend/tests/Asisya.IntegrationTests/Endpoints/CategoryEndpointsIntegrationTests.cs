using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Asisya.Application.Features.Categories.Commands.CreateCategory;
using Asisya.Application.Features.Categories.Queries.GetCategories;
using Asisya.Domain.Entities;
using Asisya.Infrastructure.Persistence;
using Asisya.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Asisya.IntegrationTests.Endpoints;

/// <summary>
/// Integration test suite exercising Category endpoints against a real ephemeral PostgreSQL database
/// managed by Testcontainers.
/// </summary>
public class CategoryEndpointsIntegrationTests : IClassFixture<AsisyaApiFactory>
{
    private readonly AsisyaApiFactory _factory;
    private readonly HttpClient _anonymousClient;
    private readonly HttpClient _adminClient;

    public CategoryEndpointsIntegrationTests(AsisyaApiFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
        _adminClient = factory.CreateAdminClient();
    }

    [Fact]
    public async Task PostCategory_ShouldCreateCategoryAndAuditLog_InRealPostgreSql()
    {
        // Arrange
        var uniqueCategory = $"STORAGE-{Guid.NewGuid():N}";
        var command = new CreateCategoryCommand
        {
            CategoryName = uniqueCategory,
            Description = "Enterprise storage array networking systems"
        };

        // Act
        var response = await _adminClient.PostAsJsonAsync("/Category", command);

        // Assert HTTP response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var categoryId = jsonDoc.RootElement.GetProperty("categoryId").GetInt32();
        categoryId.Should().BeGreaterThan(0);

        // Assert Database state in real PostgreSQL testcontainer
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AsisyaDbContext>();

        var persistedCategory = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        persistedCategory.Should().NotBeNull();
        persistedCategory!.CategoryName.Should().Be(uniqueCategory);
        persistedCategory.Description.Should().Be("Enterprise storage array networking systems");

        // Assert AuditLog was captured by AuditableEntitySaveChangesInterceptor
        var auditEntry = await db.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TableName == "Categories" && a.Action == "Insert" && a.NewValues != null && a.NewValues.Contains(uniqueCategory));

        auditEntry.Should().NotBeNull();
        auditEntry!.UserId.Should().Contain("admin");
        auditEntry.NewValues.Should().Contain(uniqueCategory);
    }

    [Fact]
    public async Task PostCategory_ShouldReturn401Unauthorized_WhenUnauthenticated()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            CategoryName = "Unauthorized Category",
            Description = "Should be rejected"
        };

        // Act
        var response = await _anonymousClient.PostAsJsonAsync("/Category", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnAllCategories_WhenAuthenticated()
    {
        // Act
        var response = await _adminClient.GetAsync("/Category");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
    }
}
