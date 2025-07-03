using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteAttribute.V1;

internal sealed class DeleteAttributeCommandValidatorV1 : AbstractValidator<DeleteAttributeCommandV1>
{
    public DeleteAttributeCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Attribute ID is required.");
    }
}