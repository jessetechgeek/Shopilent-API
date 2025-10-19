using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

[Collection("IntegrationTests")]
public class AttributeReadRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public AttributeReadRepositoryTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAttribute_ShouldReturnAttributeDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attribute = AttributeBuilder.Random().Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.GetByIdAsync(attribute.Id);

        // Assert
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
    public async Task GetByIdAsync_NonExistentAttribute_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.AttributeReader.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingName_ShouldReturnAttributeDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        var uniqueName = $"test_attribute_{DateTime.Now.Ticks}";
        var attribute = AttributeBuilder.Random()
            .WithName(uniqueName)
            .WithDisplayName("Test Attribute")
            .Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.GetByNameAsync(uniqueName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(attribute.Id);
        result.Name.Should().Be(uniqueName);
        result.DisplayName.Should().Be("Test Attribute");
    }

    [Fact]
    public async Task GetByNameAsync_NonExistentName_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentName = "non_existent_attribute";

        // Act
        var result = await _unitOfWork.AttributeReader.GetByNameAsync(nonExistentName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task NameExistsAsync_ExistingName_ShouldReturnTrue()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attributeName = $"existing_attribute_{DateTime.Now.Ticks}";
        var attribute = AttributeBuilder.Random()
            .WithName(attributeName)
            .Build();
        await _unitOfWork.AttributeWriter.AddAsync(attribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.NameExistsAsync(attributeName);

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
        var result = await _unitOfWork.AttributeReader.NameExistsAsync(nonExistentName);

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
        var result = await _unitOfWork.AttributeReader.NameExistsAsync(attributeName, attribute.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetVariantAttributesAsync_HasVariantAttributes_ShouldReturnOnlyVariantAttributes()
    {
        // Arrange
        await ResetDatabaseAsync();

        var variantAttribute1 = AttributeBuilder.VariantAttribute("size", AttributeType.Text).Build();
        var variantAttribute2 = AttributeBuilder.VariantAttribute("color", AttributeType.Text).Build();
        var regularAttribute = AttributeBuilder.Random().Build(); // Not a variant attribute

        await _unitOfWork.AttributeWriter.AddAsync(variantAttribute1);
        await _unitOfWork.AttributeWriter.AddAsync(variantAttribute2);
        await _unitOfWork.AttributeWriter.AddAsync(regularAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.GetVariantAttributesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == variantAttribute1.Id && a.IsVariant);
        result.Should().Contain(a => a.Id == variantAttribute2.Id && a.IsVariant);
        result.Should().NotContain(a => a.Id == regularAttribute.Id);
    }

    [Fact]
    public async Task GetVariantAttributesAsync_NoVariantAttributes_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        var regularAttribute1 = AttributeBuilder.Random().Build();
        var regularAttribute2 = AttributeBuilder.Random().Build();

        await _unitOfWork.AttributeWriter.AddAsync(regularAttribute1);
        await _unitOfWork.AttributeWriter.AddAsync(regularAttribute2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.GetVariantAttributesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_HasAttributes_ShouldReturnAllAttributes()
    {
        // Arrange
        await ResetDatabaseAsync();
        var attributes = AttributeBuilder.CreateMany(5);
        foreach (var attribute in attributes)
        {
            await _unitOfWork.AttributeWriter.AddAsync(attribute);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.ListAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Select(a => a.Id).Should().BeEquivalentTo(attributes.Select(a => a.Id));
    }

    [Fact]
    public async Task ListAllAsync_DifferentAttributeTypes_ShouldReturnAllTypes()
    {
        // Arrange
        await ResetDatabaseAsync();

        var textAttribute = AttributeBuilder.Random().WithType(AttributeType.Text).Build();
        var numberAttribute = AttributeBuilder.Random().WithType(AttributeType.Number).Build();
        var booleanAttribute = AttributeBuilder.Random().WithType(AttributeType.Boolean).Build();

        await _unitOfWork.AttributeWriter.AddAsync(textAttribute);
        await _unitOfWork.AttributeWriter.AddAsync(numberAttribute);
        await _unitOfWork.AttributeWriter.AddAsync(booleanAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AttributeReader.ListAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Type == AttributeType.Text);
        result.Should().Contain(a => a.Type == AttributeType.Number);
        result.Should().Contain(a => a.Type == AttributeType.Boolean);
    }


    [Fact]
    public async Task FilterableAndSearchableAttributes_ShouldHaveCorrectFlags()
    {
        // Arrange
        await ResetDatabaseAsync();

        var filterableAttribute = AttributeBuilder.FilterableAttribute("brand").Build();
        var searchableAttribute = AttributeBuilder.SearchableAttribute("description").Build();
        var bothAttribute = AttributeBuilder.Random()
            .WithName("both_attribute")
            .AsFilterable()
            .AsSearchable()
            .Build();

        await _unitOfWork.AttributeWriter.AddAsync(filterableAttribute);
        await _unitOfWork.AttributeWriter.AddAsync(searchableAttribute);
        await _unitOfWork.AttributeWriter.AddAsync(bothAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var all = await _unitOfWork.AttributeReader.ListAllAsync();

        // Assert
        all.Should().HaveCount(3);

        var filterable = all.First(a => a.Name == "brand");
        filterable.Filterable.Should().BeTrue();
        filterable.Searchable.Should().BeFalse();

        var searchable = all.First(a => a.Name == "description");
        searchable.Filterable.Should().BeFalse();
        searchable.Searchable.Should().BeTrue();

        var both = all.First(a => a.Name == "both_attribute");
        both.Filterable.Should().BeTrue();
        both.Searchable.Should().BeTrue();
    }
}
