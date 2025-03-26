using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Sales.Errors;
using Shopilent.Domain.Sales.Events;

namespace Shopilent.Domain.Sales;

public class Cart : AggregateRoot
{
    private Cart()
    {
        // Required by EF Core
    }

    private Cart(User user = null)
    {
        if (user != null)
            UserId = user.Id;

        Metadata = new Dictionary<string, object>();
        _items = new List<CartItem>();
    }

    public static Result<Cart> Create(User user = null)
    {
        var cart = new Cart(user);
        cart.AddDomainEvent(new CartCreatedEvent(cart.Id));
        return Result.Success(cart);
    }

    public static Result<Cart> CreateWithMetadata(User user, Dictionary<string, object> metadata)
    {
        if (user == null)
            return Result.Failure<Cart>(UserErrors.NotFound(Guid.Empty));

        if (metadata == null)
            return Result.Failure<Cart>(CartErrors.InvalidMetadata);

        var result = Create(user);
        if (result.IsFailure)
            return result;

        var cart = result.Value;
        foreach (var item in metadata)
        {
            cart.Metadata[item.Key] = item.Value;
        }

        return Result.Success(cart);
    }

    public Guid? UserId { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public Result AssignToUser(User user)
    {
        if (user == null)
            return Result.Failure(UserErrors.NotFound(Guid.Empty));

        UserId = user.Id;
        AddDomainEvent(new CartAssignedToUserEvent(Id, user.Id));
        return Result.Success();
    }

    public Result<CartItem> AddItem(Product product, int quantity = 1, ProductVariant variant = null)
    {
        if (product == null)
            return Result.Failure<CartItem>(ProductErrors.NotFound(Guid.Empty));

        if (quantity <= 0)
            return Result.Failure<CartItem>(CartErrors.InvalidQuantity);

        if (!product.IsActive)
            return Result.Failure<CartItem>(CartErrors.ProductUnavailable(product.Id));

        if (variant != null && !variant.IsActive)
            return Result.Failure<CartItem>(CartErrors.ProductVariantNotAvailable(variant.Id));

        // Check if the item already exists
        var existingItem = _items.Find(i =>
            i.ProductId == product.Id &&
            ((variant == null && i.VariantId == null) || (variant != null && i.VariantId == variant.Id)));

        if (existingItem != null)
        {
            var updateResult = existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            if (updateResult.IsFailure)
                return Result.Failure<CartItem>(updateResult.Error);

            AddDomainEvent(new CartItemUpdatedEvent(Id, existingItem.Id));
            return Result.Success(existingItem);
        }

        var item = CartItem.Create(this, product, quantity, variant);
        _items.Add(item);

        AddDomainEvent(new CartItemAddedEvent(Id, item.Id));
        return Result.Success(item);
    }

    public Result UpdateItemQuantity(Guid itemId, int quantity)
    {
        var item = _items.Find(i => i.Id == itemId);
        if (item == null)
            return Result.Failure(CartErrors.ItemNotFound(itemId));

        if (quantity <= 0)
        {
            return RemoveItem(itemId);
        }

        var updateResult = item.UpdateQuantity(quantity);
        if (updateResult.IsFailure)
            return updateResult;

        AddDomainEvent(new CartItemUpdatedEvent(Id, itemId));
        return Result.Success();
    }

    public Result RemoveItem(Guid itemId)
    {
        var item = _items.Find(i => i.Id == itemId);
        if (item == null)
            return Result.Failure(CartErrors.ItemNotFound(itemId));

        _items.Remove(item);
        AddDomainEvent(new CartItemRemovedEvent(Id, itemId));
        return Result.Success();
    }

    public Result Clear()
    {
        _items.Clear();
        AddDomainEvent(new CartClearedEvent(Id));
        return Result.Success();
    }

    public Result UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(CartErrors.InvalidMetadataKey);

        Metadata[key] = value;
        return Result.Success();
    }
}