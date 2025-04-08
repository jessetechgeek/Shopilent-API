using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCategory()
    {
        // Arrange
        var name = "Electronics";
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        // Act
        var result = Category.Create(name, slug);

        // Assert
        Assert.True(result.IsSuccess);
        var category = result.Value;
        Assert.Equal(name, category.Name);
        Assert.Equal(slug, category.Slug);
        Assert.Null(category.ParentId);
        Assert.Equal(0, category.Level);
        Assert.Equal($"/{slug}", category.Path);
        Assert.True(category.IsActive);
        Assert.Empty(category.Children);
        Assert.Empty(category.ProductCategories);
        Assert.Contains(category.DomainEvents, e => e is CategoryCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var name = string.Empty;
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        // Act
        var result = Category.Create(name, slug);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Category.NameRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldReturnFailure()
    {
        // Arrange
        var name = "Electronics";
        var slugResult = Slug.Create(string.Empty);

        // Act & Assert
        Assert.True(slugResult.IsFailure);
        Assert.Equal("Category.SlugRequired", slugResult.Error.Code);
    }

    [Fact]
    public void Create_WithParent_ShouldCreateCategoryWithCorrectHierarchy()
    {
        // Arrange
        var parentName = "Electronics";
        var parentSlugResult = Slug.Create("electronics");
        Assert.True(parentSlugResult.IsSuccess);
        var parentSlug = parentSlugResult.Value;

        var parentResult = Category.Create(parentName, parentSlug);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;

        var childName = "Smartphones";
        var childSlugResult = Slug.Create("smartphones");
        Assert.True(childSlugResult.IsSuccess);
        var childSlug = childSlugResult.Value;

        // Act
        var result = Category.Create(childName, childSlug, parent);

        // Assert
        Assert.True(result.IsSuccess);
        var child = result.Value;
        Assert.Equal(childName, child.Name);
        Assert.Equal(childSlug, child.Slug);
        Assert.Equal(parent.Id, child.ParentId);
        Assert.Equal(1, child.Level);
        Assert.Equal($"/{parentSlug}/{childSlug}", child.Path);
        Assert.True(child.IsActive);
    }

    [Fact]
    public void CreateInactive_ShouldCreateInactiveCategory()
    {
        // Arrange
        var name = "Electronics";
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        // Act
        var result = Category.CreateInactive(name, slug);

        // Assert
        Assert.True(result.IsSuccess);
        var category = result.Value;
        Assert.Equal(name, category.Name);
        Assert.Equal(slug, category.Slug);
        Assert.False(category.IsActive);
        Assert.Contains(category.DomainEvents, e => e is CategoryCreatedEvent);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCategory()
    {
        // Arrange
        var categoryResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        var newName = "Tech Gadgets";
        var newSlugResult = Slug.Create("tech-gadgets");
        Assert.True(newSlugResult.IsSuccess);
        var newSlug = newSlugResult.Value;

        var description = "All kinds of tech gadgets";

        // Act
        var result = category.Update(newName, newSlug, description);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, category.Name);
        Assert.Equal(newSlug, category.Slug);
        Assert.Equal(description, category.Description);
        Assert.Contains(category.DomainEvents, e => e is CategoryUpdatedEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateCategory()
    {
        // Arrange
        var categoryResult = Category.CreateInactive("Electronics", Slug.Create("electronics").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        Assert.False(category.IsActive);

        // Act
        var result = category.Activate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(category.IsActive);
        Assert.Contains(category.DomainEvents, e => e is CategoryStatusChangedEvent);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCategory()
    {
        // Arrange
        var categoryResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        Assert.True(category.IsActive);

        // Act
        var result = category.Deactivate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(category.IsActive);
        Assert.Contains(category.DomainEvents, e => e is CategoryStatusChangedEvent);
    }

    [Fact]
    public void SetParent_ShouldUpdateCategoryHierarchy()
    {
        // Arrange
        var categoryResult = Category.Create("Smartphones", Slug.Create("smartphones").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;

        // Act
        var result = category.SetParent(parent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(parent.Id, category.ParentId);
        Assert.Equal(1, category.Level);
        Assert.Equal($"/{parent.Slug}/{category.Slug}", category.Path);
        Assert.Contains(category.DomainEvents, e => e is CategoryHierarchyChangedEvent);
    }

    [Fact]
    public void SetParent_ToNull_ShouldMakeCategoryRoot()
    {
        // Arrange
        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;

        var categoryResult = Category.Create("Smartphones", Slug.Create("smartphones").Value, parent);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        Assert.Equal(parent.Id, category.ParentId);

        // Act
        var result = category.SetParent(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(category.ParentId);
        Assert.Equal(0, category.Level);
        Assert.Equal($"/{category.Slug}", category.Path);
        Assert.Contains(category.DomainEvents, e => e is CategoryHierarchyChangedEvent);
    }

    [Fact]
    public void AddChild_ShouldAddChildAndUpdateHierarchy()
    {
        // Arrange
        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;

        var childResult = Category.Create("Smartphones", Slug.Create("smartphones").Value);
        Assert.True(childResult.IsSuccess);
        var child = childResult.Value;

        // Act
        var result = parent.AddChild(child);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(parent.Id, child.ParentId);
        Assert.Equal(1, child.Level);
        Assert.Equal($"/{parent.Slug}/{child.Slug}", child.Path);
        Assert.Contains(parent.Children, c => c.Id == child.Id);
    }

    [Fact]
    public void AddChild_WithNullChild_ShouldReturnFailure()
    {
        // Arrange
        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;

        // Act
        var result = parent.AddChild(null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Category.NotFound", result.Error.Code);
    }
}