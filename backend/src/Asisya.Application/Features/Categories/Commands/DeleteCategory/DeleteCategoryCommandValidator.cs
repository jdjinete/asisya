using FluentValidation;

namespace Asisya.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Validator for <see cref="DeleteCategoryCommand"/>.
/// </summary>
public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    /// <summary>
    /// Initializes validation rules for deleting a category.
    /// </summary>
    public DeleteCategoryCommandValidator()
    {
        RuleFor(v => v.CategoryId)
            .GreaterThan(0)
            .WithMessage("CategoryId must be greater than 0.");
    }
}
