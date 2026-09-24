using MediatR;

namespace Asisya.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Command to delete a category from the catalog.
/// Enforces referential integrity: rejection if any products are assigned.
/// </summary>
public record DeleteCategoryCommand(int CategoryId) : IRequest<Unit>;
