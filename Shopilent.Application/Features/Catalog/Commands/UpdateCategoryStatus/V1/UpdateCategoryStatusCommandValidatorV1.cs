using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateCategoryStatus.V1;

internal sealed class UpdateCategoryStatusCommandValidatorV1 : AbstractValidator<UpdateCategoryStatusCommandV1>
{
    public UpdateCategoryStatusCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}