using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.RemoveItemFromCart.V1;

internal sealed class RemoveItemFromCartCommandValidatorV1 : AbstractValidator<RemoveItemFromCartCommandV1>
{
    public RemoveItemFromCartCommandValidatorV1()
    {
        RuleFor(v => v.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");
    }
}