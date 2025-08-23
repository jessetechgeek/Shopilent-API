using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.ValueObjects;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Shipping.Write;

[Collection("IntegrationTests")]
public class AddressWriteRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public AddressWriteRepositoryTests(IntegrationTestFixture fixture) : base(fixture) { }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_ValidAddress_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .WithPhoneNumber("555-123-4567")
            .AsShipping()
            .IsDefault()
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AddressReader.GetByIdAsync(address.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(address.Id);
        result.UserId.Should().Be(user.Id);
        result.AddressLine1.Should().Be(address.AddressLine1);
        result.AddressLine2.Should().Be(address.AddressLine2);
        result.City.Should().Be(address.City);
        result.State.Should().Be(address.State);
        result.Country.Should().Be(address.Country);
        result.PostalCode.Should().Be(address.PostalCode);
        result.Phone.Should().Be("5551234567"); // Phone number is normalized to digits only
        result.IsDefault.Should().BeTrue();
        result.AddressType.Should().Be(AddressType.Shipping);
        result.CreatedAt.Should().BeCloseTo(address.CreatedAt, TimeSpan.FromMilliseconds(100));
        result.UpdatedAt.Should().BeCloseTo(address.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task AddAsync_AddressWithoutPhoneNumber_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsBilling()
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AddressReader.GetByIdAsync(address.Id);
        result.Should().NotBeNull();
        result!.Phone.Should().BeNullOrEmpty();
        result.AddressType.Should().Be(AddressType.Billing);
        result.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ExistingAddress_ShouldModifyAddress()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach to simulate real-world scenario
        DbContext.Entry(address).State = EntityState.Detached;

        // Act - Load fresh entity and update
        var existingAddress = await _unitOfWork.AddressWriter.GetByIdAsync(address.Id);

        var newPostalAddress = PostalAddress.Create(
            "456 Oak Avenue",
            "New City",
            "NY",
            "US",
            "10001",
            "Apt 2B").Value;

        var newPhone = PhoneNumber.Create("555-999-8888").Value;

        var updateResult = existingAddress!.Update(newPostalAddress, newPhone);
        updateResult.IsSuccess.Should().BeTrue();

        await _unitOfWork.AddressWriter.UpdateAsync(existingAddress);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedAddress = await _unitOfWork.AddressReader.GetByIdAsync(address.Id);
        updatedAddress.Should().NotBeNull();
        updatedAddress!.AddressLine1.Should().Be("456 Oak Avenue");
        updatedAddress.AddressLine2.Should().Be("Apt 2B");
        updatedAddress.City.Should().Be("New City");
        updatedAddress.State.Should().Be("NY");
        updatedAddress.PostalCode.Should().Be("10001");
        updatedAddress.Phone.Should().Be("5559998888"); // Phone number is normalized to digits only
        updatedAddress.UpdatedAt.Should().BeAfter(updatedAddress.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ChangeAddressType_ShouldModifyAddressType()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsShipping()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach to simulate real-world scenario
        DbContext.Entry(address).State = EntityState.Detached;

        // Act - Load fresh entity and update address type
        var existingAddress = await _unitOfWork.AddressWriter.GetByIdAsync(address.Id);

        var setTypeResult = existingAddress!.SetAddressType(AddressType.Billing);
        setTypeResult.IsSuccess.Should().BeTrue();

        await _unitOfWork.AddressWriter.UpdateAsync(existingAddress);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedAddress = await _unitOfWork.AddressReader.GetByIdAsync(address.Id);
        updatedAddress.Should().NotBeNull();
        updatedAddress!.AddressType.Should().Be(AddressType.Billing);
    }

    [Fact]
    public async Task UpdateAsync_SetAsDefault_ShouldUpdateDefaultStatus()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .IsNotDefault()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach to simulate real-world scenario
        DbContext.Entry(address).State = EntityState.Detached;

        // Act - Load fresh entity and set as default
        var existingAddress = await _unitOfWork.AddressWriter.GetByIdAsync(address.Id);

        var setDefaultResult = existingAddress!.SetDefault(true);
        setDefaultResult.IsSuccess.Should().BeTrue();

        await _unitOfWork.AddressWriter.UpdateAsync(existingAddress);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedAddress = await _unitOfWork.AddressReader.GetByIdAsync(address.Id);
        updatedAddress.Should().NotBeNull();
        updatedAddress!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ExistingAddress_ShouldRemoveFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.AddressWriter.DeleteAsync(address);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AddressReader.GetByIdAsync(address.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAddress_ShouldReturnAddress()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AddressWriter.GetByIdAsync(address.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(address.Id);
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentAddress_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.AddressWriter.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingUser_ShouldReturnUserAddresses()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address1 = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .IsDefault()
            .Build();

        var address2 = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .IsNotDefault()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AddressWriter.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.UserId == user.Id);

        // Should be ordered by IsDefault DESC, then CreatedAt DESC
        result.First().IsDefault.Should().BeTrue();
        result.First().Id.Should().Be(address1.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_NonExistentUser_ShouldReturnEmpty()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.AddressWriter.GetByUserIdAsync(nonExistentUserId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDefaultAddressAsync_HasDefaultAddress_ShouldReturnDefaultAddress()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var defaultAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsShipping()
            .IsDefault()
            .Build();

        var nonDefaultAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsShipping()
            .IsNotDefault()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AddressWriter.GetDefaultAddressAsync(user.Id, AddressType.Shipping);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(defaultAddress.Id);
        result.IsDefault.Should().BeTrue();
        result.AddressType.Should().Be(AddressType.Shipping);
    }

    [Fact]
    public async Task GetDefaultAddressAsync_NoDefaultAddress_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var nonDefaultAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsShipping()
            .IsNotDefault()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AddressWriter.GetDefaultAddressAsync(user.Id, AddressType.Shipping);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDefaultAddressAsync_OnlyOneDefaultAllowed_ShouldReturnLatestDefault()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();

        // Create first default "Both" address
        var defaultBothAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsBoth()
            .IsDefault()
            .Build();

        // Create second default "Shipping" address (should become the only default)
        var defaultShippingAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsShipping()
            .IsDefault()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act - Request default address for shipping
        var result = await _unitOfWork.AddressWriter.GetDefaultAddressAsync(user.Id, AddressType.Shipping);

        // Assert - Should return the latest default address (Shipping), not the Both address
        result.Should().NotBeNull();
        result!.AddressType.Should().Be(AddressType.Shipping);
        result.Id.Should().Be(defaultShippingAddress.Id);
        
        // Verify only one default address exists
        var allAddresses = await _unitOfWork.AddressWriter.GetByUserIdAsync(user.Id);
        allAddresses.Count(a => a.IsDefault).Should().Be(1);
        allAddresses.First(a => a.Id == defaultBothAddress.Id).IsDefault.Should().BeFalse();
        allAddresses.First(a => a.Id == defaultShippingAddress.Id).IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenConcurrentUpdate_ShouldThrowConcurrencyException()
    {
        // Arrange - Create and save initial address
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var address = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act - Simulate concurrent access with two service scopes
        // Create separate service scopes to simulate true concurrent access
        using var scope1 = ServiceProvider.CreateScope();
        using var scope2 = ServiceProvider.CreateScope();

        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Both contexts load the same entity (same Version initially)
        var address1 = await unitOfWork1.AddressWriter.GetByIdAsync(address.Id);
        var address2 = await unitOfWork2.AddressWriter.GetByIdAsync(address.Id);

        // Both try to modify the entity
        var newPostalAddress1 = PostalAddress.Create("First Street", "First City", "CA", "US", "11111").Value;
        var newPostalAddress2 = PostalAddress.Create("Second Street", "Second City", "NY", "US", "22222").Value;

        address1!.Update(newPostalAddress1);
        address2!.Update(newPostalAddress2);

        // First update should succeed (Version incremented)
        await unitOfWork1.AddressWriter.UpdateAsync(address1);
        await unitOfWork1.SaveChangesAsync();

        // Second update should fail with concurrency exception (stale Version)
        await unitOfWork2.AddressWriter.UpdateAsync(address2);
        var action = () => unitOfWork2.SaveChangesAsync();

        // Assert
        await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task BulkOperations_MultipleAddresses_ShouldHandleCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var addresses = new[]
        {
            new AddressBuilder().WithUser(user).WithUniqueData().AsShipping().IsDefault().Build(),
            new AddressBuilder().WithUser(user).WithUniqueData().AsBilling().Build(),
            new AddressBuilder().WithUser(user).WithUniqueData().AsBoth().Build()
        };

        // Act - Add user with multiple addresses
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert - All addresses should be persisted
        var userAddresses = await _unitOfWork.AddressWriter.GetByUserIdAsync(user.Id);
        userAddresses.Should().HaveCount(3);

        var addressIds = userAddresses.Select(a => a.Id).ToList();
        addressIds.Should().Contain(addresses.Select(a => a.Id));

        // Default address should be first
        userAddresses.First().IsDefault.Should().BeTrue();
        userAddresses.First().AddressType.Should().Be(AddressType.Shipping);
    }

    [Fact]
    public async Task AddressTypeHandling_AllTypes_ShouldPersistCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = new UserBuilder().Build();
        var shippingAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsShipping()
            .Build();

        var billingAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsBilling()
            .Build();

        var bothAddress = new AddressBuilder()
            .WithUser(user)
            .WithUniqueData()
            .AsBoth()
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var addresses = await _unitOfWork.AddressWriter.GetByUserIdAsync(user.Id);
        addresses.Should().HaveCount(3);

        addresses.Should().ContainSingle(a => a.AddressType == AddressType.Shipping);
        addresses.Should().ContainSingle(a => a.AddressType == AddressType.Billing);
        addresses.Should().ContainSingle(a => a.AddressType == AddressType.Both);
    }
}
