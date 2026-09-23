using Asisya.Application.Common.Interfaces;
using Asisya.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Query handler executing paginated, filtered, and sorted queries on the Products catalog.
/// Implements EF Core query composition with AsNoTracking to maximize throughput.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductsQueryHandler"/> class.
    /// </summary>
    public GetProductsQueryHandler(IApplicationDbContext _context)
    {
        this._context = _context;
    }

    /// <inheritdoc />
    public async Task<PaginatedList<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => request.PageSize
        };

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        // 1. Text Search Filter (ProductName)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(search));
        }

        // 2. Category Filter
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // 3. Price Bounds Filter
        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.UnitPrice >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.UnitPrice <= request.MaxPrice.Value);
        }

        // 4. Discontinued Filter
        if (request.Discontinued.HasValue)
        {
            query = query.Where(p => p.Discontinued == request.Discontinued.Value);
        }

        // 5. Dynamic Sorting
        var isDescending = request.SortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        query = request.SortBy?.ToLower() switch
        {
            "price" => isDescending ? query.OrderByDescending(p => p.UnitPrice) : query.OrderBy(p => p.UnitPrice),
            "stock" => isDescending ? query.OrderByDescending(p => p.UnitsInStock) : query.OrderBy(p => p.UnitsInStock),
            _ => isDescending ? query.OrderByDescending(p => p.ProductName) : query.OrderBy(p => p.ProductName)
        };

        // 6. Projection to DTO
        var projected = query.Select(p => new ProductSummaryDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.CategoryName : null,
            UnitPrice = p.UnitPrice,
            UnitsInStock = p.UnitsInStock,
            Discontinued = p.Discontinued,
            QuantityPerUnit = p.QuantityPerUnit
        });

        return await PaginatedList<ProductSummaryDto>.CreateAsync(projected, pageIndex, pageSize, cancellationToken);
    }
}
