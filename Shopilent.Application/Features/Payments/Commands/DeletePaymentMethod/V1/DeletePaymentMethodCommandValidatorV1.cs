using FluentValidation;

namespace Shopilent.Application.Features.Payments.Commands.DeletePaymentMethod.V1;

internal sealed class DeletePaymentMethodCommandValidatorV1 : AbstractValidator<DeletePaymentMethodCommandV1>
{
    public DeletePaymentMethodCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Payment method ID is required.");
    }
}