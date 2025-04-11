using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog;

public class Category : AggregateRoot
{
    private Category()
    {
        // Required by EF Core
    }

    private Category(string name, Slug slug, Category parent = null)
    {
        Name = name;
        Slug = slug;

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

    public static Result<Category> Create(string name, Slug slug, Category parent = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>(CategoryErrors.NameRequired);

        if (slug == null || string.IsNullOrWhiteSpace(slug.Value))
            return Result.Failure<Category>(CategoryErrors.SlugRequired);

        var category = new Category(name, slug, parent);
        category.AddDomainEvent(new CategoryCreatedEvent(category.Id));
        return Result.Success(category);
    }

    public static Result<Category> CreateInactive(string name, Slug slug, Category parent = null)
    {
        var result = Create(name, slug, parent);
        if (result.IsFailure)
            return result;

        var category = result.Value;
        category.IsActive = false;
        return Result.Success(category);
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

    public Result Update(string name, Slug slug, string description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CategoryErrors.NameRequired);

        if (slug == null || string.IsNullOrWhiteSpace(slug.Value))
            return Result.Failure(CategoryErrors.SlugRequired);

        Name = name;
        Slug = slug;
        Description = description;
        AddDomainEvent(new CategoryUpdatedEvent(Id));
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Success();

        IsActive = true;
        AddDomainEvent(new CategoryStatusChangedEvent(Id, true));
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Success();

        IsActive = false;
        AddDomainEvent(new CategoryStatusChangedEvent(Id, false));
        return Result.Success();
    }

    public Result SetParent(Category parent)
    {
        if (parent == null)
        {
            ParentId = null;
            Level = 0;
            Path = $"/{Slug}";
        }
        else
        {
            // Check for circular reference
            var currentParent = parent;
            while (currentParent != null)
            {
                if (currentParent.Id == Id)
                    return Result.Failure(CategoryErrors.CircularReference);

                currentParent = currentParent.ParentId.HasValue ? null : null; // In real application, you would load the parent
            }

            ParentId = parent.Id;
            Level = parent.Level + 1;
            Path = $"{parent.Path}/{Slug}";
        }

        AddDomainEvent(new CategoryHierarchyChangedEvent(Id));
        return Result.Success();
    }

    public Result AddChild(Category child)
    {
        if (child == null)
            return Result.Failure(CategoryErrors.NotFound(Guid.Empty));

        var result = child.SetParent(this);
        if (result.IsFailure)
            return result;

        _children.Add(child);
        return Result.Success();
    }
    
    public Result Delete()
    {
        if (_productCategories.Any())
            return Result.Failure(CategoryErrors.CannotDeleteWithProducts);
    
        if (_children.Any())
            return Result.Failure(CategoryErrors.CannotDeleteWithChildren);
    
        AddDomainEvent(new CategoryDeletedEvent(Id));
        return Result.Success();
    }
}