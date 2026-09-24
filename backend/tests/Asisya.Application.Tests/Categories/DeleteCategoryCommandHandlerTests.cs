using Asisya.Application.Features.Categories.Commands.DeleteCategory;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Asisya.Application.Tests.Categories;

public class DeleteCategoryCommandHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new DeleteCategoryCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteCategory_WhenNoProductsAreAssigned()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 5,
            CategoryName = "Legacy Accessories",
            Description = "Deprecated accessories"
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var command = new DeleteCategoryCommand(5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.Categories.FindAsync(5);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCategoryHasAssociatedProducts()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            CategoryName = "SERVIDORES",
            Description = "Hardware"
        };
        var product = new Product
        {
            ProductId = 10,
            ProductName = "Cisco UCS Blade",
            CategoryId = 1,
            UnitPrice = 4500m,
            UnitsInStock = 2
        };

        _context.Categories.Add(category);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new DeleteCategoryCommand(1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert (Referential integrity enforcement)
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*associated products*");

        // Category must still exist
        var existing = await _context.Categories.FindAsync(1);
        existing.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new DeleteCategoryCommand(999);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
