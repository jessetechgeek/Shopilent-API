using Shopilent.Domain.Catalog;
using Shopilent.Domain.Common;
using Shopilent.Domain.Identity;
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

    public static Cart Create(User user = null)
    {
        var cart = new Cart(user);
        cart.AddDomainEvent(new CartCreatedEvent(cart.Id));
        return cart;
    }

    public static Cart CreateWithMetadata(User user, Dictionary<string, object> metadata)
    {
        var cart = Create(user);
        foreach (var item in metadata)
        {
            cart.Metadata[item.Key] = item.Value;
        }

        return cart;
    }

    public Guid? UserId { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AssignToUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        UserId = user.Id;
        AddDomainEvent(new CartAssignedToUserEvent(Id, user.Id));
    }

    public CartItem AddItem(Product product, int quantity = 1, ProductVariant variant = null)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        // Check if the item already exists
        var existingItem = _items.Find(i =>
            i.ProductId == product.Id &&
            ((variant == null && i.VariantId == null) || (variant != null && i.VariantId == variant.Id)));

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            return existingItem;
        }

        var item = CartItem.Create(this, product, quantity, variant);
        _items.Add(item);

        AddDomainEvent(new CartItemAddedEvent(Id, item.Id));
        return item;
    }

    public void UpdateItemQuantity(Guid itemId, int quantity)
    {
        var item = _items.Find(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {itemId} not found in cart.");

        if (quantity <= 0)
        {
            RemoveItem(itemId);
            return;
        }

        item.UpdateQuantity(quantity);
        AddDomainEvent(new CartItemUpdatedEvent(Id, itemId));
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.Find(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            AddDomainEvent(new CartItemRemovedEvent(Id, itemId));
        }
    }

    public void Clear()
    {
        _items.Clear();
        AddDomainEvent(new CartClearedEvent(Id));
    }

    public void UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata[key] = value;
    }
}