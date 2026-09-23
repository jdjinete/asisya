using Asisya.Application.Features.Products.Commands.DeleteProduct;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Asisya.Application.Tests.Products;

public class DeleteProductCommandHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly DeleteProductCommandHandler _handler;
    private readonly DeleteProductCommandValidator _validator;

    public DeleteProductCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new DeleteProductCommandHandler(_context);
        _validator = new DeleteProductCommandValidator();
    }

    [Fact]
    public async Task Handle_ShouldRemoveProduct_WhenProductExistsAndHasNoOrderDetails()
    {
        // Arrange
        var product = new Product { ProductName = "Item to Delete", UnitPrice = 50m };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var command = new DeleteProductCommand(product.ProductId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Products.FindAsync(product.ProductId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new DeleteProductCommand(99999);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductHasOrderDetails()
    {
        // Arrange
        var product = new Product { ProductName = "Product with Orders" };
        var customer = new Customer { CustomerId = "ALFKI", CompanyName = "Alfreds Futterkiste" };
        var order = new Order { Customer = customer };
        var orderDetail = new OrderDetail
        {
            Product = product,
            Order = order,
            UnitPrice = 25m,
            Quantity = 2
        };

        _context.Products.Add(product);
        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.OrderDetails.Add(orderDetail);
        await _context.SaveChangesAsync();

        var command = new DeleteProductCommand(product.ProductId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        ex.Message.Should().Contain("cannot be deleted because it is associated with existing commercial orders");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenProductIdIsZeroOrNegative()
    {
        var command = new DeleteProductCommand(0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
