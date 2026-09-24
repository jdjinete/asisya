using Asisya.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Handler for executing <see cref="DeleteCategoryCommand"/>.
/// Strictly validates that no products are assigned before permitting deletion (ON DELETE RESTRICT).
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCategoryCommandHandler"/> class.
    /// </summary>
    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {request.CategoryId} was not found.");
        }

        // Enforce referential integrity in application layer before database constraint check
        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == request.CategoryId, cancellationToken);

        if (hasProducts)
        {
            throw new InvalidOperationException(
                $"Cannot delete category '{category.CategoryName}' (ID: {category.CategoryId}) because it has associated products. Remove or reassign existing products first.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
