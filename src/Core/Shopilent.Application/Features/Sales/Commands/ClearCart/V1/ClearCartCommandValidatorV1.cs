using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.ClearCart.V1;

internal sealed class ClearCartCommandValidatorV1 : AbstractValidator<ClearCartCommandV1>
{
    public ClearCartCommandValidatorV1()
    {
        RuleFor(v => v.CartId)
            .NotEqual(Guid.Empty).WithMessage("Cart ID cannot be empty when provided.")
            .When(v => v.CartId.HasValue);
    }
}