using FluentValidation;

namespace Asisya.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Validator for <see cref="UpdateCategoryCommand"/> enforcing business constraints and payload integrity.
/// </summary>
public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>
    /// Initializes validation rules for updating an existing category.
    /// </summary>
    public UpdateCategoryCommandValidator()
    {
        RuleFor(v => v.CategoryId)
            .GreaterThan(0)
            .WithMessage("CategoryId must be greater than 0.");

        RuleFor(v => v.CategoryName)
            .NotEmpty().WithMessage("CategoryName is required.")
            .MaximumLength(50).WithMessage("CategoryName must not exceed 50 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
