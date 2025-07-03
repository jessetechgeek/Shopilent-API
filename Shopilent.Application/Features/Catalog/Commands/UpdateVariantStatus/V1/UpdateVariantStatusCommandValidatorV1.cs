using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariantStatus.V1;

internal sealed class UpdateVariantStatusCommandValidatorV1 : AbstractValidator<UpdateVariantStatusCommandV1>
{
    public UpdateVariantStatusCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Variant ID is required.");
    }
}