using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.ProcessOrderRefund.V1;

public class ProcessOrderRefundRequestValidatorV1 : Validator<ProcessOrderRefundRequestV1>
{
    public ProcessOrderRefundRequestValidatorV1()
    {
        RuleFor(v => v.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}