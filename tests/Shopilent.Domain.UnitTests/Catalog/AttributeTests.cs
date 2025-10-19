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
        result.IsSuccess.Should().BeTrue();
        var attribute = result.Value;
        attribute.Name.Should().Be(name);
        attribute.DisplayName.Should().Be(displayName);
        attribute.Type.Should().Be(type);
        attribute.Filterable.Should().BeFalse();
        attribute.Searchable.Should().BeFalse();
        attribute.IsVariant.Should().BeFalse();
        attribute.Configuration.Should().NotBeNull().And.BeEmpty();
        attribute.DomainEvents.Should().Contain(e => e is AttributeCreatedEvent);
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
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attribute.NameRequired");
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
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attribute.DisplayNameRequired");
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
        result.IsSuccess.Should().BeTrue();
        var attribute = result.Value;
        attribute.Name.Should().Be(name);
        attribute.DisplayName.Should().Be(displayName);
        attribute.Type.Should().Be(type);
        attribute.Filterable.Should().BeTrue();
        attribute.Searchable.Should().BeFalse();
        attribute.IsVariant.Should().BeFalse();
        attribute.DomainEvents.Should().Contain(e => e is AttributeCreatedEvent);
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
        result.IsSuccess.Should().BeTrue();
        var attribute = result.Value;
        attribute.Name.Should().Be(name);
        attribute.DisplayName.Should().Be(displayName);
        attribute.Type.Should().Be(type);
        attribute.Filterable.Should().BeFalse();
        attribute.Searchable.Should().BeTrue();
        attribute.IsVariant.Should().BeFalse();
        attribute.DomainEvents.Should().Contain(e => e is AttributeCreatedEvent);
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
        result.IsSuccess.Should().BeTrue();
        var attribute = result.Value;
        attribute.Name.Should().Be(name);
        attribute.DisplayName.Should().Be(displayName);
        attribute.Type.Should().Be(type);
        attribute.Filterable.Should().BeFalse();
        attribute.Searchable.Should().BeFalse();
        attribute.IsVariant.Should().BeTrue();
        attribute.DomainEvents.Should().Contain(e => e is AttributeCreatedEvent);
    }

    [Fact]
    public void SetFilterable_ShouldUpdateFilterableFlag()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        attributeResult.IsSuccess.Should().BeTrue();
        var attribute = attributeResult.Value;
        attribute.Filterable.Should().BeFalse();

        // Act
        var result = attribute.SetFilterable(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        attribute.Filterable.Should().BeTrue();
    }

    [Fact]
    public void SetSearchable_ShouldUpdateSearchableFlag()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        attributeResult.IsSuccess.Should().BeTrue();
        var attribute = attributeResult.Value;
        attribute.Searchable.Should().BeFalse();

        // Act
        var result = attribute.SetSearchable(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        attribute.Searchable.Should().BeTrue();
    }

    [Fact]
    public void SetIsVariant_ShouldUpdateIsVariantFlag()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        attributeResult.IsSuccess.Should().BeTrue();
        var attribute = attributeResult.Value;
        attribute.IsVariant.Should().BeFalse();

        // Act
        var result = attribute.SetIsVariant(true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        attribute.IsVariant.Should().BeTrue();
    }

    [Fact]
    public void UpdateConfiguration_ShouldAddOrUpdateConfigurationValue()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        attributeResult.IsSuccess.Should().BeTrue();
        var attribute = attributeResult.Value;
        var key = "available_values";
        var value = new[] { "Red", "Blue", "Green" };

        // Act
        var result = attribute.UpdateConfiguration(key, value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        attribute.Configuration.Should().ContainKey(key);
        attribute.Configuration[key].Should().BeEquivalentTo(value);
    }

    [Fact]
    public void UpdateConfiguration_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var attributeResult = Attribute.Create("color", "Color", AttributeType.Color);
        attributeResult.IsSuccess.Should().BeTrue();
        var attribute = attributeResult.Value;
        var key = string.Empty;
        var value = new[] { "Red", "Blue", "Green" };

        // Act
        var result = attribute.UpdateConfiguration(key, value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attribute.InvalidConfigurationFormat");
    }
}