using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateAttribute.V1;

internal sealed class UpdateAttributeCommandValidatorV1 : AbstractValidator<UpdateAttributeCommandV1>
{
    public UpdateAttributeCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Attribute ID is required.");

        RuleFor(v => v.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");
    }
}