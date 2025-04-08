using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Tests.Catalog.Events;

public class CategoryEventTests
{
    [Fact]
    public void Category_WhenCreated_ShouldRaiseCategoryCreatedEvent()
    {
        // Arrange & Act
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        
        var result = Category.Create("Electronics", slugResult.Value);

        // Assert
        Assert.True(result.IsSuccess);
        var category = result.Value;
        var domainEvent = Assert.Single(category.DomainEvents, e => e is CategoryCreatedEvent);
        var createdEvent = (CategoryCreatedEvent)domainEvent;
        Assert.Equal(category.Id, createdEvent.CategoryId);
    }

    [Fact]
    public void Category_WhenUpdated_ShouldRaiseCategoryUpdatedEvent()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var categoryResult = Category.Create("Electronics", slug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        category.ClearDomainEvents(); // Clear the creation event

        var newSlugResult = Slug.Create("updated-electronics");
        Assert.True(newSlugResult.IsSuccess);
        var newSlug = newSlugResult.Value;

        // Act
        var updateResult = category.Update("Updated Electronics", newSlug, "Updated description");
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(category.DomainEvents, e => e is CategoryUpdatedEvent);
        var updatedEvent = (CategoryUpdatedEvent)domainEvent;
        Assert.Equal(category.Id, updatedEvent.CategoryId);
    }

    [Fact]
    public void Category_WhenActivated_ShouldRaiseCategoryStatusChangedEvent()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var categoryResult = Category.CreateInactive("Electronics", slug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        category.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = category.Activate();
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(category.DomainEvents, e => e is CategoryStatusChangedEvent);
        var statusEvent = (CategoryStatusChangedEvent)domainEvent;
        Assert.Equal(category.Id, statusEvent.CategoryId);
        Assert.True(statusEvent.IsActive);
    }

    [Fact]
    public void Category_WhenDeactivated_ShouldRaiseCategoryStatusChangedEvent()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var categoryResult = Category.Create("Electronics", slug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        category.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = category.Deactivate();
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(category.DomainEvents, e => e is CategoryStatusChangedEvent);
        var statusEvent = (CategoryStatusChangedEvent)domainEvent;
        Assert.Equal(category.Id, statusEvent.CategoryId);
        Assert.False(statusEvent.IsActive);
    }

    [Fact]
    public void Category_WhenParentIsSet_ShouldRaiseCategoryHierarchyChangedEvent()
    {
        // Arrange
        var parentSlugResult = Slug.Create("electronics");
        Assert.True(parentSlugResult.IsSuccess);
        var parentSlug = parentSlugResult.Value;
        
        var parentResult = Category.Create("Electronics", parentSlug);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;
        
        var childSlugResult = Slug.Create("phones");
        Assert.True(childSlugResult.IsSuccess);
        var childSlug = childSlugResult.Value;
        
        var childResult = Category.Create("Phones", childSlug);
        Assert.True(childResult.IsSuccess);
        var child = childResult.Value;
        
        child.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = child.SetParent(parent);
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(child.DomainEvents, e => e is CategoryHierarchyChangedEvent);
        var hierarchyEvent = (CategoryHierarchyChangedEvent)domainEvent;
        Assert.Equal(child.Id, hierarchyEvent.CategoryId);
    }
}