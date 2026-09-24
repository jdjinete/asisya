using MediatR;

namespace Asisya.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product from the catalog.
/// Enforces referential integrity checks (e.g. against historical order line items) and registers an audit log.
/// </summary>
/// <param name="ProductId">Unique primary key of the product to delete.</param>
public record DeleteProductCommand(int ProductId) : IRequest<bool>;
