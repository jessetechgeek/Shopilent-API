using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Administration.Commands.RebuildSearchIndex.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Administration.RebuildSearchIndex.V1;

public class RebuildSearchIndexEndpointV1 : Endpoint<RebuildSearchIndexRequestV1, ApiResponse<RebuildSearchIndexResponseV1>>
{
    private readonly ISender _sender;

    public RebuildSearchIndexEndpointV1(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("v1/administration/search/rebuild");
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
        Description(b => b
            .WithName("RebuildSearchIndex")
            .Produces<ApiResponse<RebuildSearchIndexResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RebuildSearchIndexResponseV1>>(StatusCodes.Status500InternalServerError)
            .WithTags("Administration")
            .WithSummary("Rebuild search indexes")
            .WithDescription("Initializes search indexes and reindexes all products. This operation is restricted to administrators and managers."));
    }

    public override async Task HandleAsync(RebuildSearchIndexRequestV1 req, CancellationToken ct)
    {
        var command = new RebuildSearchIndexCommandV1
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

            var errorResponse = ApiResponse<RebuildSearchIndexResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var apiResponse = ApiResponse<RebuildSearchIndexResponseV1>.Success(
            result.Value,
            result.Value.Message);

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}