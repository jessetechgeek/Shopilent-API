using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.MarkOrderAsShipped.V1;

public class MarkOrderAsShippedRequestValidatorV1 : Validator<MarkOrderAsShippedRequestV1>
{
    public MarkOrderAsShippedRequestValidatorV1()
    {
        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.TrackingNumber));
    }
}