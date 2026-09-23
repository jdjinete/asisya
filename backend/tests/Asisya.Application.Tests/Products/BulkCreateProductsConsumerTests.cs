using Asisya.Application.Features.Products.Commands.BulkCreateProducts;
using Asisya.Application.Features.Products.Consumers;
using Asisya.Application.Features.Products.Events;
using Asisya.Application.Tests.Common;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class BulkCreateProductsConsumerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly ILogger<BulkCreateProductsConsumer> _logger;
    private readonly BulkCreateProductsConsumer _consumer;

    public BulkCreateProductsConsumerTests()
    {
        _context = TestDbContextFactory.Create();
        _logger = Substitute.For<ILogger<BulkCreateProductsConsumer>>();
        _consumer = new BulkCreateProductsConsumer(_context, _logger);
    }

    [Fact]
    public async Task Consume_ShouldGenerateAndInsertRandomProducts_InBatches()
    {
        // Arrange
        var message = new BatchProductsReceivedEvent
        {
            BatchId = Guid.NewGuid(),
            GenerateRandomCount = 2500,
            BatchSize = 500
        };

        var consumeContext = Substitute.For<ConsumeContext<BatchProductsReceivedEvent>>();
        consumeContext.Message.Returns(message);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContext);

        // Assert
        var totalInDb = await _context.Products.CountAsync();
        totalInDb.Should().Be(2500);

        var servidoresCategory = await _context.Categories.FirstAsync(c => c.CategoryName == "SERVIDORES");
        var cloudCategory = await _context.Categories.FirstAsync(c => c.CategoryName == "CLOUD");

        var servidoresCount = await _context.Products.CountAsync(p => p.CategoryId == servidoresCategory.CategoryId);
        var cloudCount = await _context.Products.CountAsync(p => p.CategoryId == cloudCategory.CategoryId);

        servidoresCount.Should().BeGreaterThan(1000);
        cloudCount.Should().BeGreaterThan(1000);
        (servidoresCount + cloudCount).Should().Be(2500);
    }

    [Fact]
    public async Task Consume_ShouldImportExplicitProductsList_Successfully()
    {
        // Arrange
        var products = new List<BulkCreateProductItemDto>
        {
            new() { ProductName = "Blade Server Model X", UnitPrice = 1200.50m, UnitsInStock = 15 },
            new() { ProductName = "Cloud VM Instance 4Core", UnitPrice = 85.00m, UnitsInStock = 100 },
            new() { ProductName = "Rackmount Switch 48P", UnitPrice = 450.00m, UnitsInStock = 30 }
        };

        var message = new BatchProductsReceivedEvent
        {
            BatchId = Guid.NewGuid(),
            Products = products,
            BatchSize = 2
        };

        var consumeContext = Substitute.For<ConsumeContext<BatchProductsReceivedEvent>>();
        consumeContext.Message.Returns(message);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContext);

        // Assert
        var count = await _context.Products.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task Consume_ShouldSkipInvalidProducts_WhenProductNameIsEmpty()
    {
        // Arrange: one invalid product name
        var products = new List<BulkCreateProductItemDto>
        {
            new() { ProductName = "Valid Product 1", UnitPrice = 10.00m },
            new() { ProductName = "", UnitPrice = 20.00m }, // Invalid
            new() { ProductName = "Valid Product 2", UnitPrice = 30.00m }
        };

        var message = new BatchProductsReceivedEvent
        {
            BatchId = Guid.NewGuid(),
            Products = products,
            BatchSize = 10
        };

        var consumeContext = Substitute.For<ConsumeContext<BatchProductsReceivedEvent>>();
        consumeContext.Message.Returns(message);
        consumeContext.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(consumeContext);

        // Assert
        var count = await _context.Products.CountAsync();
        count.Should().Be(2);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
