using FluentValidation;

namespace Asisya.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Validator for <see cref="CreateCategoryCommand"/> enforcing business rules.
/// </summary>
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>
    /// Initializes validation rules for category creation.
    /// </summary>
    public CreateCategoryCommandValidator()
    {
        RuleFor(v => v.CategoryName)
            .NotEmpty().WithMessage("CategoryName is required.")
            .MaximumLength(50).WithMessage("CategoryName must not exceed 50 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
