using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Catalog.Events;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Domain.Tests.Catalog;

public class AttributeTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateAttribute()
    {
        // Arrange
        var name = "color";
        var displayName = "Color";
        var type = AttributeType.Color;

        // Act
        var result = Attribute.Create(name, displayName, type);

        // Assert
        Assert.True(result.IsSuccess);
        var attribute = result.Value;
        Assert.Equal(name, attribute.Name);
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.Equal(type, attribute.Type);
        Assert.False(attribute.Filterable);
        Assert.False(attribute.Searchable);
        Assert.False(attribute.IsVariant);
        Assert.NotNull(attribute.Configuration);
        Assert.Empty(attribute.Configuration);
        Assert.Contains(attribute.DomainEvents, e => e is AttributeCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var name = string.Empty;
        var displayName = "Color";
        var type = AttributeType.Color;

        // Act
        var result = Attribute.Create(name, displayName, type);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Attribute.NameRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyDisplayName_ShouldReturnFailure()
    {
        // Arrange
        var name = "color";
        var displayName = string.Empty;
        var type = AttributeType.Color;

        // Act
        var result = Attribute.Create(name, displayName, type);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Attribute.DisplayNameRequired", result.Error.Code);
    }

    [Fact]
    public void CreateFilterable_ShouldCreateFilterableAttribute()
    {
        // Arrange
        var name = "color";
        var displayName = "Color";
        var type = AttributeType.Color;

        // Act
        var result = Attribute.CreateFilterable(name, displayName, type);

        // Assert
        Assert.True(result.IsSuccess);
        var attribute = result.Value;
        Assert.Equal(name, attribute.Name);
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.Equal(type, attribute.Type);
        Assert.True(attribute.Filterable);
        Assert.False(attribute.Searchable);
        Assert.False(attribute.IsVariant);
        Assert.Contains(attribute.DomainEvents, e => e is AttributeCreatedEvent);
    }

    [Fact]
    public void CreateSearchable_ShouldCreateSearchableAttribute()
    {
        // Arrange
        var name = "color";
        var displayName = "Color";
        var type = AttributeType.Color;

        // Act
        var result = Attribute.CreateSearchable(name, displayName, type);

        // Assert
        Assert.True(result.IsSuccess);
        var attribute = result.Value;
        Assert.Equal(name, attribute.Name);
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.Equal(type, attribute.Type);
        Assert.False(attribute.Filterable);
        Assert.True(attribute.Searchable);
        Assert.False(attribute.IsVariant);
        Assert.Contains(attribute.DomainEvents, e => e is AttributeCreatedEvent);
    }

    [Fact]
    public void CreateVariant_ShouldCreateVariantAttribute()
    {
        // Arrange
        var name = "color";
        var displayName = "Color";
        var type = AttributeType.Color;

        // Act
        var result = Attribute.CreateVariant(name, displayName, type);

        // Assert
        Assert.True(result.IsSuccess);
        var attribute = result.Value;
        Assert.Equal(name, attribute.Name);
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.Equal(type, attribute.Type);
        Assert.False(attribute.Filterable);
        Assert.False(attribute.Searchable);
        Assert.True(attribute.IsVariant);
        Assert.Contains(attribute.DomainEvents, e => e is AttributeCreatedEvent);
    }

    [Fact]
    public void SetFilterable_ShouldUpdateFilterableFlag()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value;
        Assert.False(attribute.Filterable);

        // Act
        var result = attribute.SetFilterable(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(attribute.Filterable);
    }

    [Fact]
    public void SetSearchable_ShouldUpdateSearchableFlag()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value;
        Assert.False(attribute.Searchable);

        // Act
        var result = attribute.SetSearchable(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(attribute.Searchable);
    }

    [Fact]
    public void SetIsVariant_ShouldUpdateIsVariantFlag()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value;
        Assert.False(attribute.IsVariant);

        // Act
        var result = attribute.SetIsVariant(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(attribute.IsVariant);
    }

    [Fact]
    public void UpdateConfiguration_ShouldAddOrUpdateConfigurationValue()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value;
        var key = "available_values";
        var value = new[] { "Red", "Blue", "Green" };

        // Act
        var result = attribute.UpdateConfiguration(key, value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(attribute.Configuration.ContainsKey(key));
        Assert.Equal(value, attribute.Configuration[key]);
    }

    [Fact]
    public void UpdateConfiguration_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value;
        var key = string.Empty;
        var value = new[] { "Red", "Blue", "Green" };

        // Act
        var result = attribute.UpdateConfiguration(key, value);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Attribute.InvalidConfigurationFormat", result.Error.Code);
    }
}