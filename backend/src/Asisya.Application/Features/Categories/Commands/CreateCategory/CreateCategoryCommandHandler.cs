using Asisya.Application.Common.Interfaces;
using Asisya.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Handler for executing <see cref="CreateCategoryCommand"/>.
/// Manages persistence, uniqueness checks, and category classification logic.
/// </summary>
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCategoryCommandHandler"/> class.
    /// </summary>
    public CreateCategoryCommandHandler(IApplicationDbContext _context)
    {
        this._context = _context;
    }

    /// <inheritdoc />
    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.CategoryName.Trim();

        // Check if category already exists (case-insensitive)
        var existing = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (existing != null)
        {
            return existing.CategoryId;
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

        var category = new Category
        {
            CategoryName = normalizedName,
            Description = request.Description?.Trim(),
            Picture = request.Picture
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.CategoryId;
    }
}
