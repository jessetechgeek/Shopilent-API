using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateProductStatus.V1;

internal sealed class UpdateProductStatusCommandValidatorV1 : AbstractValidator<UpdateProductStatusCommandV1>
{
    public UpdateProductStatusCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Product ID is required.");
    }
}