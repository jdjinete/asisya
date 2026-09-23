using Asisya.Application.Features.Products.Commands.UpdateProduct;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class UpdateProductCommandHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly UpdateProductCommandHandler _handler;
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpdateProductCommandHandler(_context);
        _validator = new UpdateProductCommandValidator();
    }

    [Fact]
    public async Task Handle_ShouldUpdateProductFields_WhenProductExists()
    {
        // Arrange
        var category = new Category { CategoryName = "SERVIDORES" };
        var product = new Product
        {
            ProductName = "Original Server",
            Category = category,
            UnitPrice = 1000m,
            UnitsInStock = 5,
            Discontinued = false
        };

        _context.Categories.Add(category);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new UpdateProductCommand
        {
            ProductId = product.ProductId,
            ProductName = "Updated Server Pro",
            CategoryId = category.CategoryId,
            UnitPrice = 1500m,
            UnitsInStock = 10,
            QuantityPerUnit = "2U Rack",
            Discontinued = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var updated = await _context.Products.FindAsync(product.ProductId);
        updated.Should().NotBeNull();
        updated!.ProductName.Should().Be("Updated Server Pro");
        updated.UnitPrice.Should().Be(1500m);
        updated.UnitsInStock.Should().Be(10);
        updated.QuantityPerUnit.Should().Be("2U Rack");
        updated.Discontinued.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            ProductId = 99999,
            ProductName = "NonExistent"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var product = new Product { ProductName = "Test Product" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new UpdateProductCommand
        {
            ProductId = product.ProductId,
            ProductName = "Test Product",
            CategoryId = 88888
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenProductNameIsEmpty()
    {
        var command = new UpdateProductCommand
        {
            ProductId = 1,
            ProductName = ""
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenUnitPriceIsNegative()
    {
        var command = new UpdateProductCommand
        {
            ProductId = 1,
            ProductName = "Valid Name",
            UnitPrice = -10m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
