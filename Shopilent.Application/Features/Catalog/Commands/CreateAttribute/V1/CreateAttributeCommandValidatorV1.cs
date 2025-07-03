using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.CreateAttribute.V1;

internal sealed class CreateAttributeCommandValidatorV1 : AbstractValidator<CreateAttributeCommandV1>
{
    public CreateAttributeCommandValidatorV1()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters.");

        RuleFor(v => v.DisplayName)
            .NotEmpty().WithMessage("Attribute display name is required.")
            .MaximumLength(100).WithMessage("Attribute display name must not exceed 100 characters.");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Attribute type is invalid.");
    }
}