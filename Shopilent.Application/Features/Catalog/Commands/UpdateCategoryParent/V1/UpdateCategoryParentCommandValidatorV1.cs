using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateCategoryParent.V1;

internal sealed class UpdateCategoryParentCommandValidatorV1 : AbstractValidator<UpdateCategoryParentCommandV1>
{
    public UpdateCategoryParentCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}