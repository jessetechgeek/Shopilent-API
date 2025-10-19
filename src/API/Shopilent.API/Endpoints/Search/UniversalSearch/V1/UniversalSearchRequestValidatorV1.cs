using FastEndpoints;
using FluentValidation;
using Shopilent.API.Common.Services;

namespace Shopilent.API.Endpoints.Search.UniversalSearch.V1;

public class UniversalSearchRequestValidatorV1 : Validator<UniversalSearchRequestV1>
{
    public UniversalSearchRequestValidatorV1()
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
        
        if (result.IsFailure)
            return false;
            
        var filters = result.Value;
        if (filters.CategorySlugs?.Any() == true)
        {
            foreach (var slug in filters.CategorySlugs)
            {
                if (!IsValidSlugFormat(slug))
                    return false;
            }
        }
        
        return true;
    }
    
    private bool IsValidSlugFormat(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;
            
        return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*$");
    }
}