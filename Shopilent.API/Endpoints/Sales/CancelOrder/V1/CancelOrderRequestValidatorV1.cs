using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.CancelOrder.V1;

internal sealed class CancelOrderRequestValidatorV1 : AbstractValidator<CancelOrderRequestV1>
{
    public CancelOrderRequestValidatorV1()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}