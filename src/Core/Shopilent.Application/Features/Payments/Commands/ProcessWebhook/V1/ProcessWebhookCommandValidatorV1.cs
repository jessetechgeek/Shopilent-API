using FluentValidation;

namespace Shopilent.Application.Features.Payments.Commands.ProcessWebhook.V1;

public class ProcessWebhookCommandValidatorV1 : AbstractValidator<ProcessWebhookCommandV1>
{
    public ProcessWebhookCommandValidatorV1()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required")
            .Must(BeValidProvider)
            .WithMessage("Provider must be one of: stripe, razorpay");

        RuleFor(x => x.WebhookPayload)
            .MaximumLength(1048576) // 1MB limit
            .WithMessage("Webhook payload cannot exceed 1MB")
            .When(x => !string.IsNullOrEmpty(x.WebhookPayload)); // Only validate if payload is provided

        RuleFor(x => x.Signature)
            .MaximumLength(500)
            .WithMessage("Signature cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Signature)); // Only validate if signature is provided

    }

    private static bool BeValidProvider(string provider)
    {
        var validProviders = new[] { "stripe", "razorpay" };
        return validProviders.Contains(provider.ToLowerInvariant());
    }
}