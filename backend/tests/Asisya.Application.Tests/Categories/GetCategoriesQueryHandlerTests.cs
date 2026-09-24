using System.Text;
using Asisya.Application.Features.Categories.Queries.GetCategories;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Categories;

public class GetCategoriesQueryHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetCategoriesQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCategories_OrderedByNameWithProductCounts()
    {
        // Arrange
        var catB = new Category
        {
            CategoryId = 1,
            CategoryName = "SERVIDORES",
            Description = "Physical rack hardware",
            Picture = Encoding.UTF8.GetBytes("test-picture")
        };
        var catA = new Category
        {
            CategoryId = 2,
            CategoryName = "CLOUD",
            Description = "Virtual infrastructure"
        };
        _context.Categories.AddRange(catB, catA);

        var product1 = new Product
        {
            ProductId = 101,
            ProductName = "Dell PowerEdge R650",
            CategoryId = 1,
            UnitPrice = 2500m,
            UnitsInStock = 5
        };
        var product2 = new Product
        {
            ProductId = 102,
            ProductName = "HPE ProLiant DL380",
            CategoryId = 1,
            UnitPrice = 3200m,
            UnitsInStock = 3
        };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        // Alphabetical order: CLOUD, then SERVIDORES
        result[0].CategoryName.Should().Be("CLOUD");
        result[0].ProductCount.Should().Be(0);
        result[0].PictureBase64.Should().BeNull();

        result[1].CategoryName.Should().Be("SERVIDORES");
        result[1].ProductCount.Should().Be(2);
        result[1].PictureBase64.Should().NotBeNullOrEmpty();
        result[1].PictureBase64.Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes("test-picture")));
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
