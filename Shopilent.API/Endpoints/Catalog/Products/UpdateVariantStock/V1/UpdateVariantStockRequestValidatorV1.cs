using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Products.UpdateVariantStock.V1;

public class UpdateVariantStockRequestValidatorV1 : Validator<UpdateVariantStockRequestV1>
{
    public UpdateVariantStockRequestValidatorV1()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }
}