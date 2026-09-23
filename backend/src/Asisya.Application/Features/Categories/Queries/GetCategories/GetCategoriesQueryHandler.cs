using Asisya.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Handler executing <see cref="GetCategoriesQuery"/> against catalog storage.
/// Returns all catalog categories with their associated product counts.
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCategoriesQueryHandler"/> class.
    /// </summary>
    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var rawCategories = await _context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.Description,
                c.Picture,
                ProductCount = c.Products.Count
            })
            .OrderBy(c => c.CategoryName)
            .ToListAsync(cancellationToken);

        return rawCategories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Description = c.Description,
            PictureBase64 = c.Picture != null ? Convert.ToBase64String(c.Picture) : null,
            ProductCount = c.ProductCount
        }).ToList();
    }
}
