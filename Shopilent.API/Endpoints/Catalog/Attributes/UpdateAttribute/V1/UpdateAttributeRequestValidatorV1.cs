using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Attributes.UpdateAttribute.V1;

public class UpdateAttributeRequestValidatorV1 : Validator<UpdateAttributeRequestV1>
{
    public UpdateAttributeRequestValidatorV1()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");
    }
}