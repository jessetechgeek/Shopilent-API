using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Search.Commands.ReindexProducts.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Search.ReindexProducts.V1;

public class ReindexProductsEndpointV1 : EndpointWithoutRequest<ApiResponse<ReindexProductsResponseV1>>
{
    private readonly ISender _sender;

    public ReindexProductsEndpointV1(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("v1/admin/search/reindex");
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
        Description(b => b
            .WithName("ReindexProducts")
            .Produces<ApiResponse<ReindexProductsResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ReindexProductsResponseV1>>(StatusCodes.Status500InternalServerError)
            .WithTags("Search", "Admin"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new ReindexProductsCommandV1();
        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<ReindexProductsResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var apiResponse = ApiResponse<ReindexProductsResponseV1>.Success(
            result.Value,
            result.Value.Message);

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}