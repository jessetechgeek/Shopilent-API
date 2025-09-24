using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Common.Exceptions;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Write;

[Collection("IntegrationTests")]
public class AttributeWriteRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public AttributeWriteRepositoryTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_ValidAttribute_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attribute = AttributeBuilder.Random().Build();

        // Act
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AttributeReader.GetByIdAsync(attribute.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(attribute.Id);
        result.Name.Should().Be(attribute.Name);
        result.DisplayName.Should().Be(attribute.DisplayName);
        result.Type.Should().Be(attribute.Type);
        result.Filterable.Should().Be(attribute.Filterable);
        result.Searchable.Should().Be(attribute.Searchable);
        result.IsVariant.Should().Be(attribute.IsVariant);
    }

    [Fact]
    public async Task AddAsync_AttributeWithUniqueName_ShouldPersistSuccessfully()
    {
        // Arrange
        await ResetDatabaseAsync();
        var uniqueName = $"unique_attribute_{DateTime.Now.Ticks}";
        var attribute = AttributeBuilder.Random()
            .WithName(uniqueName)
            .WithDisplayName("Unique Attribute")
            .Build();

        // Act
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AttributeWriter.GetByNameAsync(uniqueName);
        result.Should().NotBeNull();
        result!.Name.Should().Be(uniqueName);
        result.DisplayName.Should().Be("Unique Attribute");
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ShouldThrowException()
    {
        // Arrange
        await ResetDatabaseAsync();
        var duplicateName = $"duplicate_attribute_{DateTime.Now.Ticks}";

        var attribute1 = AttributeBuilder.Random()
            .WithName(duplicateName)
            .WithDisplayName("First Attribute")
            .Build();

        var attribute2 = AttributeBuilder.Random()
            .WithName(duplicateName)
            .WithDisplayName("Second Attribute")
            .Build();

        await _unitOfWork.AttributeWriter.AddAsync(attribute1);
        await _unitOfWork.SaveChangesAsync();

        // Act & Assert
        await _unitOfWork.AttributeWriter.AddAsync(attribute2);
        var action = () => _unitOfWork.SaveChangesAsync();
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UpdateAsync_ExistingAttribute_ShouldModifyAttribute()
    {
        // Arrange
        await ResetDatabaseAsync();
        var originalAttribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(originalAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Detach original entity to simulate real-world scenario
        DbContext.Entry(originalAttribute).State = EntityState.Detached;

        // Act - Load fresh entity and update
        var existingAttribute = await _unitOfWork.AttributeWriter.GetByIdAsync(originalAttribute.Id);
        var newDisplayName = "Updated Display Name";

        existingAttribute!.Update(newDisplayName);
        await _unitOfWork.AttributeWriter.UpdateAsync(existingAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedAttribute = await _unitOfWork.AttributeReader.GetByIdAsync(originalAttribute.Id);
        updatedAttribute.Should().NotBeNull();
        updatedAttribute!.DisplayName.Should().Be(newDisplayName);
        updatedAttribute.Name.Should().Be(originalAttribute.Name); // Name shouldn't change
        updatedAttribute.Type.Should().Be(originalAttribute.Type); // Type shouldn't change
    }

    [Fact]
    public async Task UpdateAsync_SetAttributeFlags_ShouldUpdateFlags()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Detach entity
        DbContext.Entry(attribute).State = EntityState.Detached;

        // Act - Load fresh entity and set flags
        var existingAttribute = await _unitOfWork.AttributeWriter.GetByIdAsync(attribute.Id);
        existingAttribute!.SetFilterable(true);
        existingAttribute.SetSearchable(true);
        existingAttribute.SetIsVariant(true);

        await _unitOfWork.AttributeWriter.UpdateAsync(existingAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AttributeReader.GetByIdAsync(attribute.Id);
        result.Should().NotBeNull();
        result!.Filterable.Should().BeTrue();
        result.Searchable.Should().BeTrue();
        result.IsVariant.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_UpdateConfiguration_ShouldPersistConfiguration()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Detach entity
        DbContext.Entry(attribute).State = EntityState.Detached;

        // Act - Load fresh entity and update configuration
        var existingAttribute = await _unitOfWork.AttributeWriter.GetByIdAsync(attribute.Id);
        existingAttribute!.UpdateConfiguration("min_value", 0);
        existingAttribute.UpdateConfiguration("max_value", 100);
        existingAttribute.UpdateConfiguration("step", 1);

        await _unitOfWork.AttributeWriter.UpdateAsync(existingAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AttributeWriter.GetByIdAsync(attribute.Id);
        result.Should().NotBeNull();
        result!.Configuration.Should().ContainKey("min_value");
        result.Configuration["min_value"].Should().Be(0);
        result.Configuration.Should().ContainKey("max_value");
        result.Configuration["max_value"].Should().Be(100);
        result.Configuration.Should().ContainKey("step");
        result.Configuration["step"].Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAttribute_ShouldRemoveFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.AttributeWriter.DeleteAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AttributeReader.GetByIdAsync(attribute.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAttribute_ShouldReturnAttribute()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeWriter.GetByIdAsync(attribute.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(attribute.Id);
        result.Name.Should().Be(attribute.Name);
        result.DisplayName.Should().Be(attribute.DisplayName);
        result.Type.Should().Be(attribute.Type);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentAttribute_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.AttributeWriter.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingName_ShouldReturnAttribute()
    {
        // Arrange
        await ResetDatabaseAsync();
        var uniqueName = $"get_by_name_test_{DateTime.Now.Ticks}";
        var attribute = AttributeBuilder.Random()
            .WithName(uniqueName)
            .Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeWriter.GetByNameAsync(uniqueName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(attribute.Id);
        result.Name.Should().Be(uniqueName);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistentName_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentName = "non_existent_name";

        // Act
        var result = await _unitOfWork.AttributeWriter.GetByNameAsync(nonExistentName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task NameExistsAsync_ExistingName_ShouldReturnTrue()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attributeName = $"name_exists_test_{DateTime.Now.Ticks}";
        var attribute = AttributeBuilder.Random()
            .WithName(attributeName)
            .Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeWriter.NameExistsAsync(attributeName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_NonExistentName_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentName = $"non_existent_{DateTime.Now.Ticks}";

        // Act
        var result = await _unitOfWork.AttributeWriter.NameExistsAsync(nonExistentName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task NameExistsAsync_ExistingNameWithExcludeId_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attributeName = $"exclude_test_{DateTime.Now.Ticks}";
        var attribute = AttributeBuilder.Random()
            .WithName(attributeName)
            .Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act - Exclude the current attribute ID
        var result = await _unitOfWork.AttributeWriter.NameExistsAsync(attributeName, attribute.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentUpdate_SameAttribute_ShouldHandleOptimisticConcurrency()
    {
        // Arrange
        await ResetDatabaseAsync();

        var attribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();
        var attributeId = attribute.Id;

        // Create separate service scopes to simulate true concurrent access
        using var scope1 = ServiceProvider.CreateScope();
        using var scope2 = ServiceProvider.CreateScope();

        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Get two instances of the same attribute from separate contexts
        var attribute1 = await unitOfWork1.AttributeWriter.GetByIdAsync(attributeId);
        var attribute2 = await unitOfWork2.AttributeWriter.GetByIdAsync(attributeId);

        attribute1.Should().NotBeNull();
        attribute2.Should().NotBeNull();

        // Verify both attributes have the same initial version
        attribute1!.Version.Should().Be(attribute2!.Version);

        // Modify both instances
        attribute1.Update("First Update");
        attribute2.Update("Second Update");

        // Act & Assert
        // First update should succeed
        await unitOfWork1.AttributeWriter.UpdateAsync(attribute1);
        await unitOfWork1.SaveChangesAsync();

        // Second update should fail due to concurrency conflict
        await unitOfWork2.AttributeWriter.UpdateAsync(attribute2);

        var action = () => unitOfWork2.SaveChangesAsync();
        await action.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    [Fact]
    public async Task CreateAttributeWithDifferentTypes_ShouldPersistCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var textAttribute = AttributeBuilder.Random()
            .WithType(AttributeType.Text)
            .WithName($"text_attr_{DateTime.Now.Ticks}")
            .Build();

        var numberAttribute = AttributeBuilder.Random()
            .WithType(AttributeType.Number)
            .WithName($"number_attr_{DateTime.Now.Ticks}")
            .Build();

        var booleanAttribute = AttributeBuilder.Random()
            .WithType(AttributeType.Boolean)
            .WithName($"bool_attr_{DateTime.Now.Ticks}")
            .Build();

        // Act
        await _unitOfWork.AttributeWriter.AddAsync(textAttribute);
        await _unitOfWork.AttributeWriter.AddAsync(numberAttribute);
        await _unitOfWork.AttributeWriter.AddAsync(booleanAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var textResult = await _unitOfWork.AttributeReader.GetByIdAsync(textAttribute.Id);
        var numberResult = await _unitOfWork.AttributeReader.GetByIdAsync(numberAttribute.Id);
        var booleanResult = await _unitOfWork.AttributeReader.GetByIdAsync(booleanAttribute.Id);

        textResult.Should().NotBeNull();
        textResult!.Type.Should().Be(AttributeType.Text);

        numberResult.Should().NotBeNull();
        numberResult!.Type.Should().Be(AttributeType.Number);

        booleanResult.Should().NotBeNull();
        booleanResult!.Type.Should().Be(AttributeType.Boolean);
    }

    [Fact]
    public async Task CreateVariantAttribute_ShouldSetIsVariantFlag()
    {
        // Arrange
        await ResetDatabaseAsync();
        var variantAttribute = AttributeBuilder.VariantAttribute("size", AttributeType.Text).Build();

        // Act
        await _unitOfWork.AttributeWriter.AddAsync(variantAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AttributeReader.GetByIdAsync(variantAttribute.Id);
        result.Should().NotBeNull();
        result!.IsVariant.Should().BeTrue();
        result.Name.Should().Be("size");
        result.Type.Should().Be(AttributeType.Text);
    }
}
