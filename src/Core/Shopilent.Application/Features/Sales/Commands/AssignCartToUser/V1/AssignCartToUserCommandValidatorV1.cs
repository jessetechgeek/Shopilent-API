using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.AssignCartToUser.V1;

internal sealed class AssignCartToUserCommandValidatorV1 : AbstractValidator<AssignCartToUserCommandV1>
{
    public AssignCartToUserCommandValidatorV1()
    {
        RuleFor(v => v.CartId)
            .NotEmpty().WithMessage("Cart ID is required.");
    }
}