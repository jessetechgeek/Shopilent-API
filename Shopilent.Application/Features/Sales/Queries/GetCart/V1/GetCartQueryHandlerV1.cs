using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Queries.GetCart.V1;

internal sealed class GetCartQueryHandlerV1 : IQueryHandler<GetCartQueryV1, CartDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<GetCartQueryHandlerV1> _logger;

    public GetCartQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        ILogger<GetCartQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<Result<CartDto?>> Handle(GetCartQueryV1 request, CancellationToken cancellationToken)
    {
        try
        {
            CartDto? cart = null;

            // If a specific cart ID is provided, try to get that cart
            if (request.CartId.HasValue)
            {
                cart = await _unitOfWork.CartReader.GetByIdAsync(request.CartId.Value, cancellationToken);

                // For authenticated users, verify the cart belongs to them
                if (cart != null && _currentUserContext.UserId.HasValue &&
                    cart.UserId != _currentUserContext.UserId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to access cart {CartId} belonging to user {CartOwnerId}",
                        _currentUserContext.UserId.Value, request.CartId.Value, cart.UserId);
                    cart = null; // Don't return cart that doesn't belong to user
                }
            }
            // For authenticated users, get their cart
            else if (_currentUserContext.UserId.HasValue)
            {
                cart = await _unitOfWork.CartReader.GetByUserIdAsync(_currentUserContext.UserId.Value,
                    cancellationToken);
            }

            if (cart != null)
            {
                _logger.LogInformation("Retrieved cart {CartId} for user {UserId} with {ItemCount} items",
                    cart.Id, cart.UserId ?? Guid.Empty, cart.TotalItems);
            }
            else
            {
                _logger.LogInformation("No cart found for user {UserId}",
                    _currentUserContext.UserId ?? Guid.Empty);
            }

            return Result.Success(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart for user {UserId}: {ErrorMessage}",
                _currentUserContext.UserId ?? Guid.Empty, ex.Message);

            return Result.Failure<CartDto?>(
                Error.Failure(
                    code: "Cart.GetFailed",
                    message: $"Failed to retrieve cart: {ex.Message}"));
        }
    }
}