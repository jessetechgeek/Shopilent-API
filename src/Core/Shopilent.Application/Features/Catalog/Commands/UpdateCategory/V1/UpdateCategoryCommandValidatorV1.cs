using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateCategory.V1;

internal sealed class UpdateCategoryCommandValidatorV1 : AbstractValidator<UpdateCategoryCommandV1>
{
    public UpdateCategoryCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(v => v.Slug)
            .NotEmpty().WithMessage("Category slug is required.")
            .MaximumLength(150).WithMessage("Category slug must not exceed 150 characters.")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Category slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));
    }
}