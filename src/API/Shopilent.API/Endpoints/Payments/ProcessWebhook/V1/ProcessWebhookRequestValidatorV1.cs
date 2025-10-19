using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Payments.ProcessWebhook.V1;

public class ProcessWebhookRequestValidatorV1 : Validator<ProcessWebhookRequestV1>
{
    public ProcessWebhookRequestValidatorV1()
    {
        // Note: Provider comes from route parameter, not request body
        // Payload and signature are extracted from HTTP request directly in the endpoint
        // This validator is mainly for any future request body validation
        
        // Optional: Add any basic validation if needed
        // Most validation happens in the command validator after data extraction
    }
}