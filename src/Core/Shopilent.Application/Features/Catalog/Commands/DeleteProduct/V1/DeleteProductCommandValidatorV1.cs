using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteProduct.V1;

internal sealed class DeleteProductCommandValidatorV1 : AbstractValidator<DeleteProductCommandV1>
{
    public DeleteProductCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Product ID is required.");
    }
}