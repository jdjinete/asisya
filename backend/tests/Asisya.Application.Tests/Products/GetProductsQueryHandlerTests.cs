using Asisya.Application.Features.Products.Queries.GetProducts;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class GetProductsQueryHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly GetProductsQueryHandler _handler;
    private readonly Category _categoryA;
    private readonly Category _categoryB;

    public GetProductsQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetProductsQueryHandler(_context);

        _categoryA = new Category { CategoryName = "SERVIDORES" };
        _categoryB = new Category { CategoryName = "CLOUD" };

        _context.Categories.AddRange(_categoryA, _categoryB);
        _context.SaveChanges();

        // Seed 15 test products
        var products = new List<Product>
        {
            new() { ProductName = "Alpha Server 100", CategoryId = _categoryA.CategoryId, UnitPrice = 1000m, UnitsInStock = 10, Discontinued = false },
            new() { ProductName = "Alpha Server 200", CategoryId = _categoryA.CategoryId, UnitPrice = 2000m, UnitsInStock = 5, Discontinued = false },
            new() { ProductName = "Beta Cloud VM Nano", CategoryId = _categoryB.CategoryId, UnitPrice = 15m, UnitsInStock = 100, Discontinued = false },
            new() { ProductName = "Beta Cloud VM Micro", CategoryId = _categoryB.CategoryId, UnitPrice = 30m, UnitsInStock = 50, Discontinued = false },
            new() { ProductName = "Gamma Storage Rack", CategoryId = _categoryA.CategoryId, UnitPrice = 500m, UnitsInStock = 20, Discontinued = true },
            new() { ProductName = "Delta Firewall Appliance", CategoryId = _categoryA.CategoryId, UnitPrice = 750m, UnitsInStock = 8, Discontinued = false },
            new() { ProductName = "Cloud Backup Service", CategoryId = _categoryB.CategoryId, UnitPrice = 45m, UnitsInStock = 200, Discontinued = false },
            new() { ProductName = "PowerEdge Server R750", CategoryId = _categoryA.CategoryId, UnitPrice = 3500m, UnitsInStock = 3, Discontinued = false },
            new() { ProductName = "Kubernetes Managed Cluster", CategoryId = _categoryB.CategoryId, UnitPrice = 120m, UnitsInStock = 80, Discontinued = false },
            new() { ProductName = "Blade Center Chassis", CategoryId = _categoryA.CategoryId, UnitPrice = 4500m, UnitsInStock = 2, Discontinued = false },
            new() { ProductName = "Edge Computing Gateway", CategoryId = _categoryA.CategoryId, UnitPrice = 320m, UnitsInStock = 15, Discontinued = false },
            new() { ProductName = "Object Storage Bucket S3", CategoryId = _categoryB.CategoryId, UnitPrice = 10m, UnitsInStock = 500, Discontinued = false },
            new() { ProductName = "Load Balancer Virtual", CategoryId = _categoryB.CategoryId, UnitPrice = 25m, UnitsInStock = 90, Discontinued = false },
            new() { ProductName = "Database Hosted PostgreSQL", CategoryId = _categoryB.CategoryId, UnitPrice = 80m, UnitsInStock = 40, Discontinued = false },
            new() { ProductName = "Legacy Server Gen8", CategoryId = _categoryA.CategoryId, UnitPrice = 250m, UnitsInStock = 0, Discontinued = true }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResults_WithCorrectMetadata()
    {
        // Arrange: Page 1, PageSize 5
        var query = new GetProductsQuery
        {
            PageIndex = 1,
            PageSize = 5
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.TotalItems.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm_CaseInsensitive()
    {
        // Arrange: Search for "alpha"
        var query = new GetProductsQuery
        {
            SearchTerm = "alpha",
            PageIndex = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.ProductName.Contains("Alpha"));
    }

    [Fact]
    public async Task Handle_ShouldFilterByCategoryId()
    {
        // Arrange: Filter by CLOUD category
        var query = new GetProductsQuery
        {
            CategoryId = _categoryB.CategoryId,
            PageIndex = 1,
            PageSize = 20
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(7);
        result.Items.Should().OnlyContain(p => p.CategoryId == _categoryB.CategoryId);
    }

    [Fact]
    public async Task Handle_ShouldFilterByPriceRange()
    {
        // Arrange: Price between 50 and 500
        var query = new GetProductsQuery
        {
            MinPrice = 50m,
            MaxPrice = 500m,
            PageIndex = 1,
            PageSize = 20
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(p => p.UnitPrice >= 50m && p.UnitPrice <= 500m);
    }

    [Fact]
    public async Task Handle_ShouldSortByPrice_Descending()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            SortBy = "price",
            SortOrder = "desc",
            PageIndex = 1,
            PageSize = 5
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeInDescendingOrder(p => p.UnitPrice);
        result.Items.First().UnitPrice.Should().Be(4500m);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
