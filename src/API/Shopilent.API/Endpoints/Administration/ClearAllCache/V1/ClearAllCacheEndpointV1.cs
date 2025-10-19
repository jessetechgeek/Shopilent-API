using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Administration.Commands.ClearAllCache.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Administration.ClearAllCache.V1;

public class ClearAllCacheEndpointV1 : EndpointWithoutRequest<ApiResponse<ClearAllCacheResponseV1>>
{
    private readonly IMediator _mediator;

    public ClearAllCacheEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("v1/administration/cache");
        Description(b => b
            .WithName("ClearAllCache")
            .Produces<ApiResponse<ClearAllCacheResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status500InternalServerError)
            .WithTags("Administration")
            .WithSummary("Clear all Redis cache")
            .WithDescription("Clears all cached data from Redis. This operation is restricted to administrators only."));
        Policies(nameof(AuthorizationPolicy.RequireAdmin));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new ClearAllCacheCommandV1();

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var errorResponse = new ApiResponse<ClearAllCacheResponseV1>
            {
                Succeeded = false,
                Message = result.Error.Message,
                StatusCode = StatusCodes.Status500InternalServerError,
                Errors = new[] { result.Error.Message }
            };

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var response = new ApiResponse<ClearAllCacheResponseV1>
        {
            Succeeded = true,
            Message = "Cache cleared successfully",
            StatusCode = StatusCodes.Status200OK,
            Data = result.Value
        };

        await SendAsync(response, response.StatusCode, ct);
    }
}