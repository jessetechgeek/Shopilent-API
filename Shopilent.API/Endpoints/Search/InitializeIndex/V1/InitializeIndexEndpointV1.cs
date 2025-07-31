using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Search.Commands.InitializeIndex.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Search.InitializeIndex.V1;

public class InitializeIndexEndpointV1 : EndpointWithoutRequest<ApiResponse<InitializeSearchIndexResponseV1>>
{
    private readonly ISender _sender;

    public InitializeIndexEndpointV1(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("v1/admin/search/initialize");
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
        Description(b => b
            .WithName("InitializeSearchIndex")
            .Produces<ApiResponse<InitializeSearchIndexResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InitializeSearchIndexResponseV1>>(StatusCodes.Status500InternalServerError)
            .WithTags("Search", "Admin"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new InitializeSearchIndexCommandV1();
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<InitializeSearchIndexResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var apiResponse = ApiResponse<InitializeSearchIndexResponseV1>.Success(
            result.Value,
            "Search indexes initialized successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}