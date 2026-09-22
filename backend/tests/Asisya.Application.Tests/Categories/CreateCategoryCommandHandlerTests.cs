using Asisya.Application.Features.Categories.Commands.CreateCategory;
using Asisya.Application.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Asisya.Application.Tests.Categories;

public class CreateCategoryCommandHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateCategoryCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenValidCommandProvided()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            CategoryName = "Electronics",
            Description = "Electronic enterprise accessories"
        };

        // Act
        var categoryId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        categoryId.Should().BeGreaterThan(0);
        var created = await _context.Categories.FindAsync(categoryId);
        created.Should().NotBeNull();
        created!.CategoryName.Should().Be("Electronics");
        created.Description.Should().Be("Electronic enterprise accessories");
    }

    [Fact]
    public async Task Handle_ShouldNormalizeCoreCategories_ToUppercase()
    {
        // Arrange
        var commandServidores = new CreateCategoryCommand
        {
            CategoryName = "servidores",
            Description = "Servers category"
        };

        var commandCloud = new CreateCategoryCommand
        {
            CategoryName = "cloud",
            Description = "Cloud infrastructure"
        };

        // Act
        var id1 = await _handler.Handle(commandServidores, CancellationToken.None);
        var id2 = await _handler.Handle(commandCloud, CancellationToken.None);

        // Assert
        var cat1 = await _context.Categories.FindAsync(id1);
        var cat2 = await _context.Categories.FindAsync(id2);

        cat1!.CategoryName.Should().Be("SERVIDORES");
        cat2!.CategoryName.Should().Be("CLOUD");
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingId_WhenCategoryNameAlreadyExists()
    {
        // Arrange
        var command1 = new CreateCategoryCommand { CategoryName = "SERVIDORES" };
        var id1 = await _handler.Handle(command1, CancellationToken.None);

        var command2 = new CreateCategoryCommand { CategoryName = "servidores" };

        // Act
        var id2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        id2.Should().Be(id1);
        var count = await _context.Categories.CountAsync(c => c.CategoryName == "SERVIDORES");
        count.Should().Be(1);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
