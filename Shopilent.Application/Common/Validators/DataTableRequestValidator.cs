using FluentValidation;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Common.Validators;

public sealed class DataTableRequestValidator : AbstractValidator<DataTableRequest>
{
    public DataTableRequestValidator()
    {
        RuleFor(v => v.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Length must be less than or equal to 1000.");

        RuleFor(v => v.Start)
            .GreaterThanOrEqualTo(0).WithMessage("Start must be greater than or equal to 0.");

        RuleFor(v => v.Draw)
            .GreaterThanOrEqualTo(0).WithMessage("Draw must be greater than or equal to 0.");
    }
}