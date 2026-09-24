using FluentValidation;

namespace Asisya.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Validator for DeleteProductCommand ensuring non-zero positive identifier.
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product deletion.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than zero.");
    }
}
