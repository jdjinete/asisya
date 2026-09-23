using Asisya.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// CQRS handler for UpdateProductCommand.
/// Mutates existing product state, validates referential category existence, and records database change deltas.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProductCommandHandler"/> class.
    /// </summary>
    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.ProductId} was not found.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
            {
                throw new InvalidOperationException($"Category with ID {request.CategoryId.Value} does not exist.");
            }
        }

        if (request.SupplierId.HasValue)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.SupplierId == request.SupplierId.Value, cancellationToken);

            if (!supplierExists)
            {
                throw new InvalidOperationException($"Supplier with ID {request.SupplierId.Value} does not exist.");
            }
        }

        product.ProductName = request.ProductName.Trim();
        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.QuantityPerUnit = request.QuantityPerUnit?.Trim();
        product.UnitPrice = request.UnitPrice;
        product.UnitsInStock = request.UnitsInStock;
        product.UnitsOnOrder = request.UnitsOnOrder;
        product.ReorderLevel = request.ReorderLevel;
        product.Discontinued = request.Discontinued;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
