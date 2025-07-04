using FluentValidation;

namespace Shopilent.Application.Features.Shipping.Commands.DeleteAddress.V1;

internal sealed class DeleteAddressCommandValidatorV1 : AbstractValidator<DeleteAddressCommandV1>
{
    public DeleteAddressCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Address ID is required.");
    }
}