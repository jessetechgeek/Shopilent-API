using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteProductVariant.V1;

internal sealed class DeleteProductVariantCommandValidatorV1 : AbstractValidator<DeleteProductVariantCommandV1>
{
    public DeleteProductVariantCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Product variant ID is required.");
    }
}