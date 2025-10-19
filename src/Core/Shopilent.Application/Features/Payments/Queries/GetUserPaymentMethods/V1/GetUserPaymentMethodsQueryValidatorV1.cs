using FluentValidation;

namespace Shopilent.Application.Features.Payments.Queries.GetUserPaymentMethods.V1;

internal sealed class GetUserPaymentMethodsQueryValidatorV1 : AbstractValidator<GetUserPaymentMethodsQueryV1>
{
    public GetUserPaymentMethodsQueryValidatorV1()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}