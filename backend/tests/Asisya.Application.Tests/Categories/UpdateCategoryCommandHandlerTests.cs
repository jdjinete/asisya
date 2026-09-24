using System.Text;
using Asisya.Application.Features.Categories.Commands.UpdateCategory;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Categories;

public class UpdateCategoryCommandHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpdateCategoryCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategoryProperties_WhenValid()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            CategoryName = "Original Name",
            Description = "Old description"
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var updateCommand = new UpdateCategoryCommand
        {
            CategoryId = 1,
            CategoryName = "Updated Hardware",
            Description = "New modernized description",
            Picture = Encoding.UTF8.GetBytes("new-picture")
        };

        // Act
        await _handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        var updated = await _context.Categories.FindAsync(1);
        updated.Should().NotBeNull();
        updated!.CategoryName.Should().Be("Updated Hardware");
        updated.Description.Should().Be("New modernized description");
        updated.Picture.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("new-picture"));
    }

    [Fact]
    public async Task Handle_ShouldNormalizeCoreCategoryNames()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            CategoryName = "Temporary",
            Description = "Desc"
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var updateCommand = new UpdateCategoryCommand
        {
            CategoryId = 1,
            CategoryName = "servidores",
            Description = "Enterprise servers"
        };

        // Act
        await _handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        var updated = await _context.Categories.FindAsync(1);
        updated!.CategoryName.Should().Be("SERVIDORES");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var updateCommand = new UpdateCategoryCommand
        {
            CategoryId = 999,
            CategoryName = "Nonexistent"
        };

        // Act
        var act = () => _handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenDuplicateNameExists()
    {
        // Arrange
        var cat1 = new Category { CategoryId = 1, CategoryName = "SERVIDORES" };
        var cat2 = new Category { CategoryId = 2, CategoryName = "CLOUD" };
        _context.Categories.AddRange(cat1, cat2);
        await _context.SaveChangesAsync();

        var updateCommand = new UpdateCategoryCommand
        {
            CategoryId = 2,
            CategoryName = "servidores" // Conflicting with CategoryId 1
        };

        // Act
        var act = () => _handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
