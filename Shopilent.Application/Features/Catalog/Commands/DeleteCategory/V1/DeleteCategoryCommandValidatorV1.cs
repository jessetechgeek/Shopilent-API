using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteCategory.V1;

internal sealed class DeleteCategoryCommandValidatorV1 : AbstractValidator<DeleteCategoryCommandV1>
{
    public DeleteCategoryCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}