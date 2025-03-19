using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog;

public class Category : AggregateRoot
{
    private Category()
    {
        // Required by EF Core
    }

    private Category(string name, Slug slug, Category parent = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug.Value))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));

        Name = name;
        Slug = slug; // Use Slug value object instead of string

        if (parent != null)
        {
            ParentId = parent.Id;
            Level = parent.Level + 1;
            Path = $"{parent.Path}/{slug}";
        }
        else
        {
            Level = 0;
            Path = $"/{slug}";
        }

        IsActive = true;
        _children = new List<Category>();
        _productCategories = new List<ProductCategory>();
    }

    public static Category Create(string name, Slug slug, Category parent = null)
    {
        var category = new Category(name, slug, parent);
        category.AddDomainEvent(new CategoryCreatedEvent(category.Id));
        return category;
    }

    public static Category CreateInactive(string name, Slug slug, Category parent = null)
    {
        var category = new Category(name, slug, parent);
        category.IsActive = false;
        category.AddDomainEvent(new CategoryCreatedEvent(category.Id));
        return category;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public Slug Slug { get; private set; }

    public int Level { get; private set; }
    public string Path { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    private readonly List<ProductCategory> _productCategories = new();
    public IReadOnlyCollection<ProductCategory> ProductCategories => _productCategories.AsReadOnly();

    public void Update(string name, Slug slug, string description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(slug.Value))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));

        Name = name;
        Slug = slug;
        Description = description;
        AddDomainEvent(new CategoryUpdatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AddDomainEvent(new CategoryStatusChangedEvent(Id, true));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new CategoryStatusChangedEvent(Id, false));
    }

    public void SetParent(Category parent)
    {
        if (parent == null)
        {
            ParentId = null;
            Level = 0;
            Path = $"/{Slug}";
        }
        else
        {
            ParentId = parent.Id;
            Level = parent.Level + 1;
            Path = $"{parent.Path}/{Slug}";
        }

        AddDomainEvent(new CategoryHierarchyChangedEvent(Id));
    }

    public void AddChild(Category child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        child.SetParent(this);
        _children.Add(child);
    }
}