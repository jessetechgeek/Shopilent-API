using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Catalog.Repositories.Write;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.Repositories.Write;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.DTOs;
using Shopilent.Domain.Sales.Errors;
using Shopilent.Domain.Sales.Repositories.Write;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping.Errors;
using Shopilent.Domain.Shipping.Repositories.Write;

namespace Shopilent.Application.Features.Sales.Commands.CreateOrderFromCart.V1;

internal sealed class
    CreateOrderFromCartCommandHandlerV1 : ICommandHandler<CreateOrderFromCartCommandV1, CreateOrderFromCartResponseV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<CreateOrderFromCartCommandHandlerV1> _logger;

    public CreateOrderFromCartCommandHandlerV1(
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        ILogger<CreateOrderFromCartCommandHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<Result<CreateOrderFromCartResponseV1>> Handle(
        CreateOrderFromCartCommandV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating order from cart for user {UserId}", _currentUserContext.UserId);

            // Get current user
            if (!_currentUserContext.UserId.HasValue)
                return Result.Failure<CreateOrderFromCartResponseV1>(UserErrors.NotAuthenticated);

            var user = await _unitOfWork.UserWriter.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);
            if (user == null)
                return Result.Failure<CreateOrderFromCartResponseV1>(
                    UserErrors.NotFound(_currentUserContext.UserId.Value));

            // Get user's cart
            Cart cart;
            if (request.CartId.HasValue)
            {
                cart = await _unitOfWork.CartWriter.GetByIdAsync(request.CartId.Value, cancellationToken);
                if (cart == null)
                    return Result.Failure<CreateOrderFromCartResponseV1>(CartErrors.CartNotFound(request.CartId.Value));

                // Verify cart belongs to user
                if (cart.UserId != user.Id)
                    return Result.Failure<CreateOrderFromCartResponseV1>(CartErrors.CartNotFound(request.CartId.Value));
            }
            else
            {
                cart = await _unitOfWork.CartWriter.GetByUserIdAsync(user.Id, cancellationToken);
                if (cart == null)
                    return Result.Failure<CreateOrderFromCartResponseV1>(CartErrors.EmptyCart);
            }

            // Validate cart has items
            if (!cart.Items.Any())
                return Result.Failure<CreateOrderFromCartResponseV1>(CartErrors.EmptyCart);

            // Get shipping address
            var shippingAddress =
                await _unitOfWork.AddressWriter.GetByIdAsync(request.ShippingAddressId, cancellationToken);
            if (shippingAddress == null)
                return Result.Failure<CreateOrderFromCartResponseV1>(AddressErrors.NotFound(request.ShippingAddressId));

            // Verify shipping address belongs to user
            if (shippingAddress.UserId != user.Id)
                return Result.Failure<CreateOrderFromCartResponseV1>(AddressErrors.NotFound(request.ShippingAddressId));

            // Get billing address (use shipping address if not provided)
            Domain.Shipping.Address billingAddress;
            if (request.BillingAddressId.HasValue)
            {
                billingAddress =
                    await _unitOfWork.AddressWriter.GetByIdAsync(request.BillingAddressId.Value, cancellationToken);
                if (billingAddress == null)
                    return Result.Failure<CreateOrderFromCartResponseV1>(
                        AddressErrors.NotFound(request.BillingAddressId.Value));

                // Verify billing address belongs to user
                if (billingAddress.UserId != user.Id)
                    return Result.Failure<CreateOrderFromCartResponseV1>(
                        AddressErrors.NotFound(request.BillingAddressId.Value));
            }
            else
            {
                billingAddress = shippingAddress;
            }

            // Calculate order totals (this would typically involve tax and shipping calculation services)
            var subtotal = CalculateSubtotal(cart);
            var tax = CalculateTax(cart, shippingAddress); // You'd implement tax calculation logic
            var shippingCost =
                CalculateShippingCost(cart, shippingAddress,
                    request.ShippingMethod); // You'd implement shipping calculation logic

            // Create order
            var orderResult = Order.Create(
                user,
                shippingAddress,
                billingAddress,
                subtotal,
                tax,
                shippingCost,
                request.ShippingMethod);

            if (orderResult.IsFailure)
                return Result.Failure<CreateOrderFromCartResponseV1>(orderResult.Error);

            var order = orderResult.Value;

            // Add cart items to order
            foreach (var cartItem in cart.Items)
            {
                // Get the actual product for current pricing and details
                var product = await _unitOfWork.ProductWriter.GetByIdAsync(cartItem.ProductId, cancellationToken);
                if (product == null)
                    return Result.Failure<CreateOrderFromCartResponseV1>(ProductErrors.NotFound(cartItem.ProductId));

                // Get variant if specified
                Domain.Catalog.ProductVariant variant = null;
                if (cartItem.VariantId.HasValue)
                {
                    variant = await _unitOfWork.ProductVariantWriter.GetByIdAsync(cartItem.VariantId.Value,
                        cancellationToken);
                    if (variant == null)
                        return Result.Failure<CreateOrderFromCartResponseV1>(
                            ProductVariantErrors.NotFound(cartItem.VariantId.Value));
                }

                // Determine unit price (variant price takes precedence over product base price)
                var unitPrice = variant?.Price ?? product.BasePrice;

                var addItemResult = order.AddItem(
                    product,
                    cartItem.Quantity,
                    unitPrice,
                    variant
                );

                if (addItemResult.IsFailure)
                    return Result.Failure<CreateOrderFromCartResponseV1>(addItemResult.Error);
            }

            // Add metadata if provided
            if (request.Metadata != null)
            {
                foreach (var metadata in request.Metadata)
                {
                    order.Metadata[metadata.Key] = metadata.Value;
                }
            }

            // Save order
            await _unitOfWork.OrderWriter.AddAsync(order, cancellationToken);

            // Clear cart after successful order creation
            var clearCartResult = cart.Clear();
            if (clearCartResult.IsSuccess)
            {
                await _unitOfWork.CartWriter.UpdateAsync(cart, cancellationToken);
            }

            // **CRITICAL: Commit the transaction to save to database**
            await _unitOfWork.SaveEntitiesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully from cart {CartId}", order.Id, cart.Id);

            // Map to response
            var response = new CreateOrderFromCartResponseV1
            {
                Id = order.Id,
                UserId = order.UserId,
                BillingAddressId = order.BillingAddressId,
                ShippingAddressId = order.ShippingAddressId,
                Subtotal = order.Subtotal.Amount,
                Tax = order.Tax.Amount,
                ShippingCost = order.ShippingCost.Amount,
                Total = order.Total.Amount,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                ShippingMethod = order.ShippingMethod,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice.Amount,
                    TotalPrice = item.TotalPrice.Amount,
                    Currency = item.UnitPrice.Currency,
                    ProductData = item.ProductData,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt
                }).ToList(),
                CreatedAt = order.CreatedAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order from cart for user {UserId}", _currentUserContext.UserId);
            return Result.Failure<CreateOrderFromCartResponseV1>(
                Domain.Common.Errors.Error.Failure(
                    code: "CreateOrderFromCart.Failed",
                    message: $"Failed to create order from cart: {ex.Message}"));
        }
    }

    private Money CalculateSubtotal(Cart cart)
    {
        // This is a simplified calculation
        // In a real implementation, you'd fetch current product/variant prices
        decimal total = 0;

        // For now, use a base price per item
        // In production, you'd get actual prices from products/variants
        foreach (var item in cart.Items)
        {
            // Simplified: assume $10 per item, multiply by quantity
            total += item.Quantity * 10.0m;
        }

        return Money.Create(total, "USD").Value; // Assuming USD currency
    }

    private Money CalculateTax(Cart cart, Domain.Shipping.Address shippingAddress)
    {
        // Implement tax calculation based on shipping address
        // This is a simplified version
        var subtotal = CalculateSubtotal(cart);
        var taxRate = 0.08m; // 8% tax rate - you'd calculate this based on address
        return Money.Create(subtotal.Amount * taxRate, subtotal.Currency).Value;
    }

    private Money CalculateShippingCost(Cart cart, Domain.Shipping.Address shippingAddress, string? shippingMethod)
    {
        // Implement shipping cost calculation
        // This is a simplified version
        decimal shippingCost = shippingMethod?.ToLower() switch
        {
            "express" => 15.00m,
            "overnight" => 25.00m,
            _ => 5.00m // Standard shipping
        };

        return Money.Create(shippingCost, "USD").Value;
    }
}