using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariantStock.V1;

internal sealed class UpdateVariantStockCommandValidatorV1 : AbstractValidator<UpdateVariantStockCommandV1>
{
    public UpdateVariantStockCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Variant ID is required.");

        RuleFor(v => v.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }
}