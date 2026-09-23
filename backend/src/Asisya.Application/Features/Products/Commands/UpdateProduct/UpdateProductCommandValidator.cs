using FluentValidation;

namespace Asisya.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Validator for UpdateProductCommand enforcing data integrity and catalog constraints.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product updates.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than zero.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName is required.")
            .MaximumLength(100).WithMessage("ProductName must not exceed 100 characters.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue)
            .WithMessage("UnitPrice cannot be negative.");

        RuleFor(x => x.UnitsInStock)
            .GreaterThanOrEqualTo((short)0).When(x => x.UnitsInStock.HasValue)
            .WithMessage("UnitsInStock cannot be negative.");

        RuleFor(x => x.UnitsOnOrder)
            .GreaterThanOrEqualTo((short)0).When(x => x.UnitsOnOrder.HasValue)
            .WithMessage("UnitsOnOrder cannot be negative.");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo((short)0).When(x => x.ReorderLevel.HasValue)
            .WithMessage("ReorderLevel cannot be negative.");
    }
}
