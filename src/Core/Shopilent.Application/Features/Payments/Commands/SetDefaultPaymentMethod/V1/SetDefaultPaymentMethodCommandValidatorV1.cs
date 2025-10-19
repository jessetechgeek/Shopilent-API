using FluentValidation;

namespace Shopilent.Application.Features.Payments.Commands.SetDefaultPaymentMethod.V1;

internal sealed class SetDefaultPaymentMethodCommandValidatorV1 : AbstractValidator<SetDefaultPaymentMethodCommandV1>
{
    public SetDefaultPaymentMethodCommandValidatorV1()
    {
        RuleFor(v => v.PaymentMethodId)
            .NotEmpty().WithMessage("Payment method ID is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}