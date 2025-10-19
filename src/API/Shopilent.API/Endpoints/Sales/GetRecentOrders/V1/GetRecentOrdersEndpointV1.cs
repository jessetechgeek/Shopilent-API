using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Queries.GetRecentOrders.V1;

namespace Shopilent.API.Endpoints.Sales.GetRecentOrders.V1;

public class GetRecentOrdersEndpointV1 : Endpoint<GetRecentOrdersRequestV1, ApiResponse<GetRecentOrdersResponseV1>>
{
    private readonly IMediator _mediator;

    public GetRecentOrdersEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/orders/recent");
        Description(b => b
            .WithName("GetRecentOrders")
            .Produces<ApiResponse<GetRecentOrdersResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetRecentOrdersResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GetRecentOrdersResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<GetRecentOrdersResponseV1>>(StatusCodes.Status403Forbidden)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(GetRecentOrdersRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<GetRecentOrdersResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var query = new GetRecentOrdersQueryV1
        {
            Count = req.Count
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<GetRecentOrdersResponseV1>.Failure(
                new[] { result.Error.Message },
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var response = ApiResponse<GetRecentOrdersResponseV1>.Success(
            new GetRecentOrdersResponseV1
            {
                Orders = result.Value,
                Count = result.Value.Count,
                RetrievedAt = DateTime.UtcNow
            },
            $"Retrieved {result.Value.Count} recent orders for dashboard");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}