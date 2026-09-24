using Asisya.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Handler for executing <see cref="UpdateCategoryCommand"/>.
/// Manages persistence, duplicate checks, and field updates.
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCategoryCommandHandler"/> class.
    /// </summary>
    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {request.CategoryId} was not found.");
        }

        var normalizedName = request.CategoryName.Trim();

        // Check if another category already uses this name (case-insensitive)
        var duplicateExists = await _context.Categories
            .AnyAsync(c => c.CategoryId != request.CategoryId &&
                           c.CategoryName.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"Another category named '{normalizedName}' already exists.");
        }

        // Apply standard casing for core infrastructure categories
        if (normalizedName.Equals("SERVIDORES", StringComparison.OrdinalIgnoreCase))
        {
            normalizedName = "SERVIDORES";
        }
        else if (normalizedName.Equals("CLOUD", StringComparison.OrdinalIgnoreCase))
        {
            normalizedName = "CLOUD";
        }

        category.CategoryName = normalizedName;
        category.Description = request.Description?.Trim();
        if (request.Picture != null)
        {
            category.Picture = request.Picture;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
