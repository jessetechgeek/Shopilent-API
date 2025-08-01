using FastEndpoints;
using FluentValidation;
using Shopilent.API.Common.Services;

namespace Shopilent.API.Endpoints.Catalog.Products.GetPaginatedProducts.V1;

public class GetPaginatedProductsRequestValidatorV1 : Validator<GetPaginatedProductsRequestV1>
{
    public GetPaginatedProductsRequestValidatorV1()
    {
        RuleFor(x => x.FiltersBase64)
            .NotEmpty().WithMessage("FiltersBase64 is required.")
            .Must(BeValidBase64String).WithMessage("FiltersBase64 must be a valid base64 encoded string.")
            .Must(BeValidFilterJson).WithMessage("FiltersBase64 must contain valid filter JSON.");
    }

    private bool BeValidBase64String(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool BeValidFilterJson(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        var filterEncodingService = new FilterEncodingService();
        var result = filterEncodingService.DecodeFilters(base64String);
        return result.IsSuccess;
    }
}