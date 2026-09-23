using Asisya.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query handler retrieving complete product information along with category photo and supplier metadata.
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductByIdQueryHandler"/> class.
    /// </summary>
    public GetProductByIdQueryHandler(IApplicationDbContext _context)
    {
        this._context = _context;
    }

    /// <inheritdoc />
    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductId == request.Id, cancellationToken);

        if (product == null)
        {
            return null;
        }

        return new ProductDetailDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            QuantityPerUnit = product.QuantityPerUnit,
            UnitPrice = product.UnitPrice,
            UnitsInStock = product.UnitsInStock,
            UnitsOnOrder = product.UnitsOnOrder,
            ReorderLevel = product.ReorderLevel,
            Discontinued = product.Discontinued,
            SupplierId = product.SupplierId,
            SupplierName = product.Supplier?.CompanyName,
            Category = product.Category != null
                ? new CategoryDetailDto
                {
                    CategoryId = product.Category.CategoryId,
                    CategoryName = product.Category.CategoryName,
                    Description = product.Category.Description,
                    Picture = product.Category.Picture
                }
                : null
        };
    }
}
