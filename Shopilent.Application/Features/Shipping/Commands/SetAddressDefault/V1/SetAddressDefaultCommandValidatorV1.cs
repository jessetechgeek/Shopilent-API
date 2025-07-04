using FluentValidation;

namespace Shopilent.Application.Features.Shipping.Commands.SetAddressDefault.V1;

internal sealed class SetAddressDefaultCommandValidatorV1 : AbstractValidator<SetAddressDefaultCommandV1>
{
    public SetAddressDefaultCommandValidatorV1()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Address ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Address ID must be a valid GUID.");
    }
}