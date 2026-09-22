using Asisya.Application.Features.Products.Queries.GetProductById;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class GetProductByIdQueryHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetProductByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductDetail_WithCategoryAndPicture()
    {
        // Arrange
        var dummyPicture = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF header sample
        var category = new Category
        {
            CategoryName = "SERVIDORES",
            Description = "High performance server systems",
            Picture = dummyPicture
        };
        var supplier = new Supplier
        {
            CompanyName = "Dell Global Logistics"
        };
        var product = new Product
        {
            ProductName = "PowerEdge R750xs",
            Category = category,
            Supplier = supplier,
            UnitPrice = 2499.99m,
            UnitsInStock = 12,
            UnitsOnOrder = 2,
            ReorderLevel = 5,
            QuantityPerUnit = "1U Rack Chassis",
            Discontinued = false
        };

        _context.Categories.Add(category);
        _context.Suppliers.Add(supplier);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var query = new GetProductByIdQuery(product.ProductId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ProductId.Should().Be(product.ProductId);
        result.ProductName.Should().Be("PowerEdge R750xs");
        result.UnitPrice.Should().Be(2499.99m);
        result.SupplierName.Should().Be("Dell Global Logistics");
        result.Category.Should().NotBeNull();
        result.Category!.CategoryName.Should().Be("SERVIDORES");
        result.Category.Picture.Should().BeEquivalentTo(dummyPicture);
        result.Category.PictureBase64.Should().StartWith("data:image/jpeg;base64,");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var query = new GetProductByIdQuery(99999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
