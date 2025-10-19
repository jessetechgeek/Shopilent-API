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
        slugResult.IsSuccess.Should().BeTrue();
        var slug = slugResult.Value;

        // Act
        var result = Category.Create(name, slug);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var category = result.Value;
        category.Name.Should().Be(name);
        category.Slug.Should().Be(slug);
        category.ParentId.Should().BeNull();
        category.Level.Should().Be(0);
        category.Path.Should().Be($"/{slug}");
        category.IsActive.Should().BeTrue();
        category.Children.Should().BeEmpty();
        category.ProductCategories.Should().BeEmpty();
        category.DomainEvents.Should().Contain(e => e is CategoryCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var name = string.Empty;
        var slugResult = Slug.Create("electronics");
        slugResult.IsSuccess.Should().BeTrue();
        var slug = slugResult.Value;

        // Act
        var result = Category.Create(name, slug);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NameRequired");
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldReturnFailure()
    {
        // Arrange
        var name = "Electronics";
        var slugResult = Slug.Create(string.Empty);

        // Act & Assert
        slugResult.IsFailure.Should().BeTrue();
        slugResult.Error.Code.Should().Be("Category.SlugRequired");
    }

    [Fact]
    public void Create_WithParent_ShouldCreateCategoryWithCorrectHierarchy()
    {
        // Arrange
        var parentName = "Electronics";
        var parentSlugResult = Slug.Create("electronics");
        parentSlugResult.IsSuccess.Should().BeTrue();
        var parentSlug = parentSlugResult.Value;

        var parentResult = Category.Create(parentName, parentSlug);
        parentResult.IsSuccess.Should().BeTrue();
        var parent = parentResult.Value;

        var childName = "Smartphones";
        var childSlugResult = Slug.Create("smartphones");
        childSlugResult.IsSuccess.Should().BeTrue();
        var childSlug = childSlugResult.Value;

        // Act
        var result = Category.Create(childName, childSlug, parent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var child = result.Value;
        child.Name.Should().Be(childName);
        child.Slug.Should().Be(childSlug);
        child.ParentId.Should().Be(parent.Id);
        child.Level.Should().Be(1);
        child.Path.Should().Be($"/{parentSlug}/{childSlug}");
        child.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateInactive_ShouldCreateInactiveCategory()
    {
        // Arrange
        var name = "Electronics";
        var slugResult = Slug.Create("electronics");
        slugResult.IsSuccess.Should().BeTrue();
        var slug = slugResult.Value;

        // Act
        var result = Category.CreateInactive(name, slug);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var category = result.Value;
        category.Name.Should().Be(name);
        category.Slug.Should().Be(slug);
        category.IsActive.Should().BeFalse();
        category.DomainEvents.Should().Contain(e => e is CategoryCreatedEvent);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCategory()
    {
        // Arrange
        var categoryResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;

        var newName = "Tech Gadgets";
        var newSlugResult = Slug.Create("tech-gadgets");
        newSlugResult.IsSuccess.Should().BeTrue();
        var newSlug = newSlugResult.Value;

        var description = "All kinds of tech gadgets";

        // Act
        var result = category.Update(newName, newSlug, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(newName);
        category.Slug.Should().Be(newSlug);
        category.Description.Should().Be(description);
        category.DomainEvents.Should().Contain(e => e is CategoryUpdatedEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateCategory()
    {
        // Arrange
        var categoryResult = Category.CreateInactive("Electronics", Slug.Create("electronics").Value);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;
        category.IsActive.Should().BeFalse();

        // Act
        var result = category.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeTrue();
        category.DomainEvents.Should().Contain(e => e is CategoryStatusChangedEvent);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCategory()
    {
        // Arrange
        var categoryResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;
        category.IsActive.Should().BeTrue();

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
        category.DomainEvents.Should().Contain(e => e is CategoryStatusChangedEvent);
    }

    [Fact]
    public void SetParent_ShouldUpdateCategoryHierarchy()
    {
        // Arrange
        var categoryResult = Category.Create("Smartphones", Slug.Create("smartphones").Value);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;

        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        parentResult.IsSuccess.Should().BeTrue();
        var parent = parentResult.Value;

        // Act
        var result = category.SetParent(parent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.ParentId.Should().Be(parent.Id);
        category.Level.Should().Be(1);
        category.Path.Should().Be($"/{parent.Slug}/{category.Slug}");
        category.DomainEvents.Should().Contain(e => e is CategoryHierarchyChangedEvent);
    }

    [Fact]
    public void SetParent_ToNull_ShouldMakeCategoryRoot()
    {
        // Arrange
        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        parentResult.IsSuccess.Should().BeTrue();
        var parent = parentResult.Value;

        var categoryResult = Category.Create("Smartphones", Slug.Create("smartphones").Value, parent);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;
        category.ParentId.Should().Be(parent.Id);

        // Act
        var result = category.SetParent(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.ParentId.Should().BeNull();
        category.Level.Should().Be(0);
        category.Path.Should().Be($"/{category.Slug}");
        category.DomainEvents.Should().Contain(e => e is CategoryHierarchyChangedEvent);
    }

    [Fact]
    public void AddChild_ShouldAddChildAndUpdateHierarchy()
    {
        // Arrange
        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        parentResult.IsSuccess.Should().BeTrue();
        var parent = parentResult.Value;

        var childResult = Category.Create("Smartphones", Slug.Create("smartphones").Value);
        childResult.IsSuccess.Should().BeTrue();
        var child = childResult.Value;

        // Act
        var result = parent.AddChild(child);

        // Assert
        result.IsSuccess.Should().BeTrue();
        child.ParentId.Should().Be(parent.Id);
        child.Level.Should().Be(1);
        child.Path.Should().Be($"/{parent.Slug}/{child.Slug}");
        parent.Children.Should().Contain(c => c.Id == child.Id);
    }

    [Fact]
    public void AddChild_WithNullChild_ShouldReturnFailure()
    {
        // Arrange
        var parentResult = Category.Create("Electronics", Slug.Create("electronics").Value);
        parentResult.IsSuccess.Should().BeTrue();
        var parent = parentResult.Value;

        // Act
        var result = parent.AddChild(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
    }
}