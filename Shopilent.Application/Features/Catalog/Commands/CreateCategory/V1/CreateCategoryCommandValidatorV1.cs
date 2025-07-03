using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;

internal sealed class CreateCategoryCommandValidatorV1 : AbstractValidator<CreateCategoryCommandV1>
{
    public CreateCategoryCommandValidatorV1()
    {
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