using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Shopilent.API.Common.Extensions;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Payments.Commands.ProcessWebhook.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Payments.ProcessWebhook.V1;

public class ProcessWebhookEndpointV1 : EndpointWithoutRequest<ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public ProcessWebhookEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/payments/webhooks/{provider}/process");
        Description(b => b
            .WithName("ProcessWebhook")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ApiResponse<string>>(StatusCodes.Status500InternalServerError)
            .WithTags("Payments"));
        AllowAnonymous(); // Webhooks don't use user authentication, they use signature verification
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Extract provider from route
        var provider = Route<string>("provider");

        // Get raw body content for webhook processing
        var rawBody = await HttpContext.Request.GetRawBodyStringAsync();

        // Debug logging
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ProcessWebhookEndpointV1>>();
        logger.LogInformation("Raw body length: {Length}, Content: {Content}",
            rawBody?.Length ?? 0,
            string.IsNullOrEmpty(rawBody) ? "[EMPTY]" : rawBody[..Math.Min(100, rawBody.Length)] + "...");

        // // Validate we have webhook payload
        if (string.IsNullOrEmpty(rawBody))
        {
            var errorResponse = ApiResponse<string>.Failure(
                "No webhook payload received", StatusCodes.Status400BadRequest);
            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Extract headers that might be needed for signature verification
        var headers = new Dictionary<string, string>();
        foreach (var header in HttpContext.Request.Headers)
        {
            // Include common webhook headers
            if (header.Key.StartsWith("Stripe-", StringComparison.OrdinalIgnoreCase) ||
                header.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                headers[header.Key] = header.Value.ToString();
            }
        }

        // Get signature from header (Stripe uses Stripe-Signature header)
        var signature = HttpContext.Request.Headers["Stripe-Signature"].FirstOrDefault() ??
                        HttpContext.Request.Headers["X-Webhook-Signature"].FirstOrDefault() ??
                        string.Empty;

        var command = new ProcessWebhookCommandV1
        {
            Provider = provider,
            WebhookPayload = rawBody,
            Signature = signature,
            Headers = headers
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var response = result.Error.Type switch
            {
                ErrorType.Validation => ApiResponse<string>.Failure(
                    result.Error.Message, StatusCodes.Status400BadRequest),
                ErrorType.Unauthorized => ApiResponse<string>.Failure(
                    result.Error.Message, StatusCodes.Status401Unauthorized),
                _ => ApiResponse<string>.Failure(
                    result.Error.Message, StatusCodes.Status500InternalServerError)
            };

            await SendAsync(response, response.StatusCode, ct);
            return;
        }

        var successResponse = ApiResponse<string>.Success("OK", "Webhook processed successfully");
        await SendAsync(successResponse, StatusCodes.Status200OK, ct);
    }
}