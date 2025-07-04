using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.ProcessOrderPartialRefund.V1;

public class ProcessOrderPartialRefundRequestValidatorV1 : Validator<ProcessOrderPartialRefundRequestV1>
{
    public ProcessOrderPartialRefundRequestValidatorV1()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Refund amount must be greater than 0")
            .PrecisionScale(12, 2, false)
            .WithMessage("Amount cannot have more than 2 decimal places");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a valid 3-letter ISO code")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase letters only");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}