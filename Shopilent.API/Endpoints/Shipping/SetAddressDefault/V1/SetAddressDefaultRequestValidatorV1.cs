using FluentValidation;

namespace Shopilent.API.Endpoints.Shipping.SetAddressDefault.V1;

internal sealed class SetAddressDefaultRequestValidatorV1 : AbstractValidator<SetAddressDefaultRequestV1>
{
    public SetAddressDefaultRequestValidatorV1()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Address ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Address ID must be a valid GUID.");
    }
}