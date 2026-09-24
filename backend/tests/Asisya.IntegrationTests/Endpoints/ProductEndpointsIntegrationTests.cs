using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Asisya.Application.Common.Models;
using Asisya.Application.Features.Products.Commands.UpdateProduct;
using Asisya.Application.Features.Products.Queries.GetProductById;
using Asisya.Application.Features.Products.Queries.GetProducts;
using Asisya.Domain.Entities;
using Asisya.Infrastructure.Persistence;
using Asisya.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Asisya.IntegrationTests.Endpoints;

/// <summary>
/// Integration test suite exercising Product catalog endpoints against an ephemeral PostgreSQL instance.
/// Validates anonymous catalog reading, RFC 7807 ProblemDetails on missing resources,
/// and authenticated data mutations with audit logging.
/// </summary>
public class ProductEndpointsIntegrationTests : IClassFixture<AsisyaApiFactory>
{
    private readonly AsisyaApiFactory _factory;
    private readonly HttpClient _anonymousClient;
    private readonly HttpClient _adminClient;

    public ProductEndpointsIntegrationTests(AsisyaApiFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
        _adminClient = factory.CreateAdminClient();
    }

    private async Task<Product> SeedProductAsync(string name, decimal price)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AsisyaDbContext>();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryName == "SERVIDORES");
        if (category == null)
        {
            category = new Category
            {
                CategoryName = "SERVIDORES",
                Description = "Servidores y racks"
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        }

        var product = new Product
        {
            ProductName = name,
            CategoryId = category.CategoryId,
            UnitPrice = price,
            UnitsInStock = 25,
            Discontinued = false
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPaginatedCatalog_Anonymously()
    {
        // Arrange: Seed product in ephemeral database with unique name
        var uniqueName = $"PowerEdge-{Guid.NewGuid():N}";
        var seeded = await SeedProductAsync(uniqueName, 4200.00m);

        // Act: Anonymous request to public catalog endpoint
        var response = await _anonymousClient.GetAsync($"/Products?searchTerm={uniqueName}&pageIndex=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paginated = await response.Content.ReadFromJsonAsync<PaginatedList<ProductSummaryDto>>();
        paginated.Should().NotBeNull();
        paginated!.Items.Should().Contain(p => p.ProductName == uniqueName);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProductDetail_WhenExists()
    {
        // Arrange
        var seeded = await SeedProductAsync("Supermicro Ultra Server", 3100.50m);

        // Act
        var response = await _anonymousClient.GetAsync($"/Products/{seeded.ProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        detail.Should().NotBeNull();
        detail!.ProductId.Should().Be(seeded.ProductId);
        detail.ProductName.Should().Be("Supermicro Ultra Server");
    }

    [Fact]
    public async Task GetProductById_ShouldReturnRfc7807ProblemDetails_WhenNotFound()
    {
        // Act: Query non-existent ID
        var response = await _anonymousClient.GetAsync("/Products/888888");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        root.GetProperty("title").GetString().Should().Be("Product Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);
        root.GetProperty("detail").GetString().Should().Contain("888888");
    }

    [Fact]
    public async Task PutProduct_ShouldUpdateProductAndRecordAuditLog_WhenAuthenticated()
    {
        // Arrange
        var seeded = await SeedProductAsync("HPE ProLiant DL380", 2800.00m);
        var updateCommand = new UpdateProductCommand
        {
            ProductId = seeded.ProductId,
            ProductName = "HPE ProLiant DL380 Gen11 Ultra",
            CategoryId = seeded.CategoryId,
            UnitPrice = 3500.00m,
            UnitsInStock = 12
        };

        // Act
        var response = await _adminClient.PutAsJsonAsync($"/Products/{seeded.ProductId}", updateCommand);

        // Assert HTTP response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert database state in PostgreSQL
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AsisyaDbContext>();

        var updated = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == seeded.ProductId);
        updated.Should().NotBeNull();
        updated!.ProductName.Should().Be("HPE ProLiant DL380 Gen11 Ultra");
        updated.UnitPrice.Should().Be(3500.00m);

        // Assert AuditLog was generated
        var audit = await db.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TableName == "Products" && a.Action == "Update" && a.PrimaryKey != null && a.PrimaryKey.Contains(seeded.ProductId.ToString()));

        audit.Should().NotBeNull();
        audit!.NewValues.Should().Contain("HPE ProLiant DL380 Gen11 Ultra");
    }
}
