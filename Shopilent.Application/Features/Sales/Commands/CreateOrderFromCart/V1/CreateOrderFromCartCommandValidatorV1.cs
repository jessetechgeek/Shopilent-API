using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.CreateOrderFromCart.V1;

internal sealed class CreateOrderFromCartCommandValidatorV1 : AbstractValidator<CreateOrderFromCartCommandV1>
{
    public CreateOrderFromCartCommandValidatorV1()
    {
        RuleFor(v => v.ShippingAddressId)
            .NotEmpty().WithMessage("Shipping address is required.");

        RuleFor(v => v.ShippingMethod)
            .MaximumLength(100).WithMessage("Shipping method must not exceed 100 characters.")
            .When(v => !string.IsNullOrEmpty(v.ShippingMethod));
    }
}