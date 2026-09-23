using MediatR;

namespace Asisya.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to retrieve single product details by identifier, including the category photo.
/// Implements requirements for GET /Products/{id}.
/// </summary>
/// <param name="Id">The unique product identifier.</param>
public record GetProductByIdQuery(int Id) : IRequest<ProductDetailDto?>;
