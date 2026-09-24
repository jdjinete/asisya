using Asisya.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// CQRS handler for DeleteProductCommand.
/// Enforces referential integrity constraints preventing orphan orders and removes the entity.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteProductCommandHandler"/> class.
    /// </summary>
    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.ProductId} was not found.");
        }

        // Enforce referential integrity: Check if product has historical order line items
        var hasOrderDetails = await _context.OrderDetails
            .AnyAsync(od => od.ProductId == request.ProductId, cancellationToken);

        if (hasOrderDetails)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} cannot be deleted because it is associated with existing commercial orders.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
