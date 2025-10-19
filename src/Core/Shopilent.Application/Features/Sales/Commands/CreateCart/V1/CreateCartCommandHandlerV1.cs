using MediatR;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Sales;

namespace Shopilent.Application.Features.Sales.Commands.CreateCart.V1;

internal sealed class CreateCartCommandHandlerV1 : IRequestHandler<CreateCartCommandV1, Result<CreateCartResponseV1>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateCartCommandHandlerV1(
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext)
    {
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<CreateCartResponseV1>> Handle(CreateCartCommandV1 request,
        CancellationToken cancellationToken)
    {
        // Get current user
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
        {
            // Create anonymous cart
            var anonymousCartResult = Cart.Create();
            if (anonymousCartResult.IsFailure)
                return Result.Failure<CreateCartResponseV1>(anonymousCartResult.Error);

            var anonymousCart = anonymousCartResult.Value;

            // Add metadata if provided
            if (request.Metadata != null)
            {
                foreach (var item in request.Metadata)
                {
                    var metadataResult = anonymousCart.UpdateMetadata(item.Key, item.Value);
                    if (metadataResult.IsFailure)
                        return Result.Failure<CreateCartResponseV1>(metadataResult.Error);
                }
            }

            // Save cart
            var savedAnonymousCart = await _unitOfWork.CartWriter.AddAsync(anonymousCart, cancellationToken);

            // Return response
            return Result.Success(new CreateCartResponseV1
            {
                Id = savedAnonymousCart.Id,
                UserId = null,
                ItemCount = savedAnonymousCart.Items.Count,
                Metadata = savedAnonymousCart.Metadata,
                CreatedAt = savedAnonymousCart.CreatedAt
            });
        }

        // Get user for authenticated cart
        var user = await _unitOfWork.UserWriter.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result.Failure<CreateCartResponseV1>(
                Domain.Identity.Errors.UserErrors.NotFound(userId.Value));
        }

        // Check if user already has an active cart
        var existingCart = await _unitOfWork.CartWriter.GetByUserIdAsync(userId.Value, cancellationToken);
        if (existingCart != null)
        {
            // Return existing cart instead of creating a new one
            return Result.Success(new CreateCartResponseV1
            {
                Id = existingCart.Id,
                UserId = existingCart.UserId,
                ItemCount = existingCart.Items.Count,
                Metadata = existingCart.Metadata,
                CreatedAt = existingCart.CreatedAt
            });
        }

        // Create new cart for authenticated user
        Result<Cart> cartResult;
        if (request.Metadata != null)
        {
            cartResult = Cart.CreateWithMetadata(user, request.Metadata);
        }
        else
        {
            cartResult = Cart.Create(user);
        }

        if (cartResult.IsFailure)
            return Result.Failure<CreateCartResponseV1>(cartResult.Error);

        var cart = cartResult.Value;

        // Save cart
        var savedCart = await _unitOfWork.CartWriter.AddAsync(cart, cancellationToken);

        // Return response
        return Result.Success(new CreateCartResponseV1
        {
            Id = savedCart.Id,
            UserId = savedCart.UserId,
            ItemCount = savedCart.Items.Count,
            Metadata = savedCart.Metadata,
            CreatedAt = savedCart.CreatedAt
        });
    }
}