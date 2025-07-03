using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Categories.UpdateCategory.V1;

public class UpdateCategoryRequestValidatorV1 : Validator<UpdateCategoryRequestV1>
{
    public UpdateCategoryRequestValidatorV1()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Category slug is required.")
            .MaximumLength(150).WithMessage("Category slug must not exceed 150 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Category slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}