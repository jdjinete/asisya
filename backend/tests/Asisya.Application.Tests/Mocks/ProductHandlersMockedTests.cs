using Asisya.Application.Common.Interfaces;
using Asisya.Application.Features.Products.Commands.DeleteProduct;
using Asisya.Application.Features.Products.Commands.UpdateProduct;
using Asisya.Application.Features.Products.Queries.GetProducts;
using Asisya.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Asisya.Application.Tests.Mocks;

/// <summary>
/// Unit test suite demonstrating strict dependency isolation and mocking using Moq.
/// Simulates <see cref="IApplicationDbContext"/> without touching an in-memory or physical database.
/// </summary>
public class ProductHandlersMockedTests
{
    [Fact]
    public async Task GetProductsQueryHandler_ShouldReturnPaginatedList_UsingMockedDbContext()
    {
        // Arrange
        var productsData = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Server Alpha", UnitPrice = 100m, UnitsInStock = 5 },
            new() { ProductId = 2, ProductName = "Server Beta", UnitPrice = 200m, UnitsInStock = 10 },
            new() { ProductId = 3, ProductName = "Cloud VM 1", UnitPrice = 300m, UnitsInStock = 15 },
            new() { ProductId = 4, ProductName = "Cloud VM 2", UnitPrice = 400m, UnitsInStock = 20 },
            new() { ProductId = 5, ProductName = "Firewall Appliance", UnitPrice = 500m, UnitsInStock = 25 }
        };

        var mockDbSet = productsData.BuildMockDbSet();

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);

        var handler = new GetProductsQueryHandler(mockContext.Object);
        var query = new GetProductsQuery
        {
            PageIndex = 2,
            PageSize = 2
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.PageIndex.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalItems.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProductCommandHandler_ShouldMutateProductAndCommit_UsingMockedDbContext()
    {
        // Arrange
        var category = new Category { CategoryId = 10, CategoryName = "SERVIDORES" };
        var product = new Product
        {
            ProductId = 42,
            ProductName = "Original Name",
            CategoryId = 10,
            UnitPrice = 50m
        };

        var productsList = new List<Product> { product };
        var categoriesList = new List<Category> { category };

        var mockProductsDbSet = productsList.BuildMockDbSet();
        var mockCategoriesDbSet = categoriesList.BuildMockDbSet();

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);
        mockContext.Setup(c => c.Categories).Returns(mockCategoriesDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

        var handler = new UpdateProductCommandHandler(mockContext.Object);
        var command = new UpdateProductCommand
        {
            ProductId = 42,
            ProductName = "Updated Server Name",
            CategoryId = 10,
            UnitPrice = 99.99m,
            UnitsInStock = 20
        };

        // Act
        var success = await handler.Handle(command, CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        product.ProductName.Should().Be("Updated Server Name");
        product.UnitPrice.Should().Be(99.99m);
        product.UnitsInStock.Should().Be(20);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductCommandHandler_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var emptyProducts = new List<Product>();
        var mockProductsDbSet = emptyProducts.BuildMockDbSet();

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);

        var handler = new UpdateProductCommandHandler(mockContext.Object);
        var command = new UpdateProductCommand
        {
            ProductId = 9999,
            ProductName = "Ghost Product"
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*9999*");

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductCommandHandler_ShouldRemoveProductAndSave_UsingMockedDbContext()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 77,
            ProductName = "Deletable Item"
        };

        var productsList = new List<Product> { product };
        var orderDetailsList = new List<OrderDetail>(); // No commercial orders referencing product

        var mockProductsDbSet = productsList.BuildMockDbSet();
        var mockOrderDetailsDbSet = orderDetailsList.BuildMockDbSet();

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);
        mockContext.Setup(c => c.OrderDetails).Returns(mockOrderDetailsDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

        var handler = new DeleteProductCommandHandler(mockContext.Object);
        var command = new DeleteProductCommand(77);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        mockProductsDbSet.Verify(m => m.Remove(It.Is<Product>(p => p.ProductId == 77)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
