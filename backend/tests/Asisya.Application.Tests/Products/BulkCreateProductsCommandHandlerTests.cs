using Asisya.Application.Features.Products.Commands.BulkCreateProducts;
using Asisya.Application.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class BulkCreateProductsCommandHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly BulkCreateProductsCommandHandler _handler;

    public BulkCreateProductsCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new BulkCreateProductsCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldGenerateAndInsertRandomProducts_InBatches()
    {
        // Arrange: Generate 2,500 products with a batch size of 500
        var command = new BulkCreateProductsCommand
        {
            GenerateRandomCount = 2500,
            BatchSize = 500
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(2500);
        result.SuccessfulImports.Should().Be(2500);
        result.FailedImports.Should().Be(0);
        result.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(0);

        var totalInDb = await _context.Products.CountAsync();
        totalInDb.Should().Be(2500);

        // Verify products are distributed across SERVIDORES and CLOUD
        var servidoresCategory = await _context.Categories.FirstAsync(c => c.CategoryName == "SERVIDORES");
        var cloudCategory = await _context.Categories.FirstAsync(c => c.CategoryName == "CLOUD");

        var servidoresCount = await _context.Products.CountAsync(p => p.CategoryId == servidoresCategory.CategoryId);
        var cloudCount = await _context.Products.CountAsync(p => p.CategoryId == cloudCategory.CategoryId);

        servidoresCount.Should().BeGreaterThan(1000);
        cloudCount.Should().BeGreaterThan(1000);
        (servidoresCount + cloudCount).Should().Be(2500);
    }

    [Fact]
    public async Task Handle_ShouldImportExplicitProductsList_Successfully()
    {
        // Arrange
        var products = new List<BulkCreateProductItemDto>
        {
            new() { ProductName = "Blade Server Model X", UnitPrice = 1200.50m, UnitsInStock = 15 },
            new() { ProductName = "Cloud VM Instance 4Core", UnitPrice = 85.00m, UnitsInStock = 100 },
            new() { ProductName = "Rackmount Switch 48P", UnitPrice = 450.00m, UnitsInStock = 30 }
        };

        var command = new BulkCreateProductsCommand
        {
            Products = products,
            BatchSize = 2
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(3);
        result.SuccessfulImports.Should().Be(3);
        result.FailedImports.Should().Be(0);

        var count = await _context.Products.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldRecordFailedImports_WhenProductNameIsInvalid()
    {
        // Arrange: one invalid product name
        var products = new List<BulkCreateProductItemDto>
        {
            new() { ProductName = "Valid Product 1", UnitPrice = 10.00m },
            new() { ProductName = "", UnitPrice = 20.00m }, // Invalid
            new() { ProductName = "Valid Product 2", UnitPrice = 30.00m }
        };

        var command = new BulkCreateProductsCommand
        {
            Products = products,
            BatchSize = 10
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(3);
        result.SuccessfulImports.Should().Be(2);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().Contain(e => e.Contains("empty or null"));

        var count = await _context.Products.CountAsync();
        count.Should().Be(2);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
