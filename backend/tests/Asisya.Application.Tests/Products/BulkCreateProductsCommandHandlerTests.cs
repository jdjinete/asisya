using Asisya.Application.Features.Products.Commands.BulkCreateProducts;
using Asisya.Application.Features.Products.Events;
using FluentAssertions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class BulkCreateProductsCommandHandlerTests
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BulkCreateProductsCommandHandler> _logger;
    private readonly BulkCreateProductsCommandHandler _handler;

    public BulkCreateProductsCommandHandlerTests()
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<BulkCreateProductsCommandHandler>>();
        _handler = new BulkCreateProductsCommandHandler(_publishEndpoint, _logger);
    }

    [Fact]
    public async Task Handle_ShouldPublishBatchProductsReceivedEvent_WhenRandomCountSpecified()
    {
        // Arrange
        var command = new BulkCreateProductsCommand
        {
            GenerateRandomCount = 2500,
            BatchSize = 500
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Accepted");
        result.TotalProcessed.Should().Be(2500);
        result.BatchId.Should().NotBeEmpty();

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<BatchProductsReceivedEvent>(e =>
                e.BatchId == result.BatchId &&
                e.GenerateRandomCount == 2500 &&
                e.BatchSize == 500),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishBatchProductsReceivedEvent_WhenProductsListSpecified()
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
        result.Should().NotBeNull();
        result.Status.Should().Be("Accepted");
        result.TotalProcessed.Should().Be(3);
        result.BatchId.Should().NotBeEmpty();

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<BatchProductsReceivedEvent>(e =>
                e.BatchId == result.BatchId &&
                e.Products != null &&
                e.Products.Count == 3 &&
                e.BatchSize == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenBothProductsAndRandomCountMissing()
    {
        // Arrange
        var command = new BulkCreateProductsCommand
        {
            GenerateRandomCount = null,
            Products = null
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Either 'GenerateRandomCount'*or 'Products'*");

        await _publishEndpoint.DidNotReceiveWithAnyArgs().Publish<BatchProductsReceivedEvent>(default!);
    }
}
