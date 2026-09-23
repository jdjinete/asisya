using MediatR;

namespace Asisya.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Query to retrieve all categories in the catalog ordered alphabetically,
/// along with their assigned product counts.
/// </summary>
public record GetCategoriesQuery : IRequest<List<CategoryDto>>;
