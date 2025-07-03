using FastEndpoints;
using FluentValidation;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;

public class CreateAttributeRequestValidatorV1 : Validator<CreateAttributeRequestV1>
{
    public CreateAttributeRequestValidatorV1()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(BeValidAttributeType)
            .WithMessage(
                "Type is invalid. Valid values are: Text, Number, Boolean, Select, Color, Date, Dimensions, Weight.");
    }

    private bool BeValidAttributeType(string type)
    {
        return Enum.TryParse<AttributeType>(type, true, out _);
    }
}