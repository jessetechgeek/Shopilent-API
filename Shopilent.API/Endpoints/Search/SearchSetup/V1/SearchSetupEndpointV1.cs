using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Search.Commands.SearchSetup.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Search.SearchSetup.V1;

public class SearchSetupEndpointV1 : Endpoint<SearchSetupRequestV1, ApiResponse<SearchSetupResponseV1>>
{
    private readonly ISender _sender;

    public SearchSetupEndpointV1(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("v1/admin/search/setup");
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
        Description(b => b
            .WithName("SearchSetup")
            .Produces<ApiResponse<SearchSetupResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SearchSetupResponseV1>>(StatusCodes.Status500InternalServerError)
            .WithTags("Search", "Admin"));
    }

    public override async Task HandleAsync(SearchSetupRequestV1 req, CancellationToken ct)
    {
        var command = new SearchSetupCommandV1
        {
            InitializeIndexes = req.InitializeIndexes,
            IndexProducts = req.IndexProducts,
            ForceReindex = req.ForceReindex
        };

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<SearchSetupResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var apiResponse = ApiResponse<SearchSetupResponseV1>.Success(
            result.Value,
            result.Value.Message);

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}