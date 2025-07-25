using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Payments.Commands.AddPaymentMethod.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Payments.AddPaymentMethod.V1;

public class AddPaymentMethodEndpointV1 : Endpoint<AddPaymentMethodRequestV1, ApiResponse<AddPaymentMethodResponseV1>>
{
    private readonly IMediator _mediator;

    public AddPaymentMethodEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/payment-methods");
        Description(b => b
            .WithName("AddPaymentMethod")
            .WithSummary("Add a new payment method with optional 3DS authentication")
            .WithDescription("Creates a new payment method for the authenticated user. Supports 3D Secure (3DS) authentication flow for European cards and other payment methods requiring additional verification.")
            .Produces<ApiResponse<AddPaymentMethodResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<AddPaymentMethodResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AddPaymentMethodResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<AddPaymentMethodResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<AddPaymentMethodResponseV1>>(StatusCodes.Status409Conflict)
            .WithTags("Payment Methods"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(AddPaymentMethodRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<AddPaymentMethodResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new AddPaymentMethodCommandV1
        {
            Type = req.Type,
            Provider = req.Provider,
            DisplayName = req.DisplayName,
            IsDefault = req.IsDefault,
            CardBrand = req.CardBrand,
            LastFourDigits = req.LastFourDigits,
            ExpiryDate = req.ExpiryDate,
            Email = req.Email,
            Metadata = req.Metadata ?? new Dictionary<string, object>(),
            PaymentMethodToken = req.PaymentMethodToken,
            RequiresSetupIntent = req.RequiresSetupIntent,
            SetupIntentId = req.SetupIntentId
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<AddPaymentMethodResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var response = ApiResponse<AddPaymentMethodResponseV1>.Success(
            result.Value,
            result.Value.RequiresAuthentication ? "3DS authentication required" : "Payment method added successfully");

        var responseStatusCode = result.Value.RequiresAuthentication ? StatusCodes.Status200OK : StatusCodes.Status201Created;
        await SendAsync(response, responseStatusCode, ct);
    }
}