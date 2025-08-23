using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Identity.Write;

[Collection("IntegrationTests")]
public class UserWriteRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public UserWriteRepositoryTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_ValidUser_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("test@example.com")
            .WithFullName("John", "Doe")
            .AsCustomer()
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var persistedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        persistedUser.Should().NotBeNull();
        persistedUser!.Email.Should().Be("test@example.com");
        persistedUser.FirstName.Should().Be("John");
        persistedUser.LastName.Should().Be("Doe");
        persistedUser.Role.Should().Be(UserRole.Customer);
        persistedUser.IsActive.Should().BeTrue();
        persistedUser.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_AdminUser_ShouldPersistWithAdminRole()
    {
        // Arrange
        await ResetDatabaseAsync();

        var adminUser = UserBuilder.AdminUser()
            .WithEmail("admin@example.com")
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(adminUser);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var persistedUser = await _unitOfWork.UserReader.GetByIdAsync(adminUser.Id);
        persistedUser.Should().NotBeNull();
        persistedUser!.Email.Should().Be("admin@example.com");
        persistedUser.Role.Should().Be(UserRole.Admin);
        persistedUser.EmailVerified.Should().BeTrue(); // AdminUser builder sets email as verified
    }

    [Fact]
    public async Task AddAsync_UserWithPhoneNumber_ShouldPersistPhoneNumber()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithPhoneNumber("+1234567890")
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var persistedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        persistedUser.Should().NotBeNull();
        persistedUser.Phone.Should().NotBeNull();
        persistedUser.Phone.Should().Be("+1234567890");
    }

    [Fact]
    public async Task AddAsync_DuplicateEmail_ShouldThrowException()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user1 = UserBuilder.Random().WithEmail("duplicate@example.com").Build();
        var user2 = UserBuilder.Random().WithEmail("duplicate@example.com").Build();

        await _unitOfWork.UserWriter.AddAsync(user1);
        await _unitOfWork.SaveChangesAsync();

        // Act & Assert
        await _unitOfWork.UserWriter.AddAsync(user2);

        var act = () => DbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_ShouldModifyUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("original@example.com")
            .WithFullName("Original", "Name")
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        DbContext.Entry(user).State = EntityState.Detached;

        // Modify the user
        var newEmail = Email.Create("updated@example.com").Value;
        var newFullName = FullName.Create("Updated", "Name").Value;
        var existingUser = await _unitOfWork.UserWriter.GetByIdAsync(user.Id);
        existingUser.UpdatePersonalInfo(newFullName, existingUser.Phone);
        existingUser.UpdateEmail(newEmail);

        // Act
        await _unitOfWork.UserWriter.UpdateAsync(existingUser);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.Email.Should().Be("updated@example.com");
        updatedUser.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("Name");
        updatedUser.UpdatedAt.Should().BeAfter(updatedUser.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ActivateUser_ShouldUpdateActiveStatus()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().AsInactive().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach the entity
        DbContext.Entry(user).State = EntityState.Detached;

        // Activate the user
        user.Activate();

        // Act
        await _unitOfWork.UserWriter.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_DeactivateUser_ShouldUpdateActiveStatus()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().AsActive().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach the entity
        DbContext.Entry(user).State = EntityState.Detached;

        // Deactivate the user
        user.Deactivate();

        // Act
        await _unitOfWork.UserWriter.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_VerifyEmail_ShouldUpdateEmailVerificationStatus()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithUnverifiedEmail().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach the entity
        DbContext.Entry(user).State = EntityState.Detached;

        // Verify email
        user.VerifyEmail();

        // Act
        await _unitOfWork.UserWriter.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ShouldRemoveFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.UserWriter.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var deletedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var retrievedUser = await _unitOfWork.UserWriter.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.Id.Should().Be(user.Id);
        retrievedUser.Email.Value.Should().Be(user.Email.Value);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var retrievedUser = await _unitOfWork.UserWriter.GetByIdAsync(nonExistentId);

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ShouldReturnUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("test@example.com")
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var retrievedUser = await _unitOfWork.UserWriter.GetByEmailAsync("test@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser.Id.Should().Be(user.Id);
        retrievedUser.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var retrievedUser = await _unitOfWork.UserWriter.GetByEmailAsync("nonexistent@example.com");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ShouldReturnUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("Test@Example.COM")
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var retrievedUser = await _unitOfWork.UserWriter.GetByEmailAsync("test@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
        // The repository should find the user regardless of case, but may normalize the email
        retrievedUser!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task ChangeRole_CustomerToAdmin_ShouldUpdateRole()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CustomerUser().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Detach the entity
        DbContext.Entry(user).State = EntityState.Detached;

        // Change role
        user.SetRole(UserRole.Admin);

        // Act
        await _unitOfWork.UserWriter.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WithNewPhoneNumber_ShouldPersistPhoneNumber()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.SaveChangesAsync();

        // Update personal info with phone number
        var newFullName = FullName.Create("Updated", "Name").Value;
        var phoneNumber = PhoneNumber.Create("+9876543210").Value;
        user.UpdatePersonalInfo(newFullName, phoneNumber);

        // Act
        await _unitOfWork.UserWriter.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("Name");
        updatedUser.Phone.Should().NotBeNull();
        updatedUser.Phone.Should().Be("+9876543210");
    }

    [Fact]
    public async Task BulkOperations_AddMultipleUsers_ShouldPersistAllUsers()
    {
        // Arrange
        await ResetDatabaseAsync();

        var users = UserBuilder.CreateMany(10);

        // Act
        foreach (var user in users)
        {
            await _unitOfWork.UserWriter.AddAsync(user);
        }

        await _unitOfWork.SaveChangesAsync();

        // Assert
        var persistedUsers = await _unitOfWork.UserReader.ListAllAsync();
        persistedUsers.Should().HaveCount(10);

        foreach (var user in users)
        {
            persistedUsers.Where(u => u.Id == user.Id).Should().ContainSingle();
        }
    }

    [Fact]
    public async Task ConcurrentUpdate_ShouldHandleOptimisticConcurrency()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        var userId = user.Id;

        // Create separate service scopes to simulate true concurrent access
        using var scope1 = ServiceProvider.CreateScope();
        using var scope2 = ServiceProvider.CreateScope();

        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Get two instances of the same user from separate contexts
        var user1 = await unitOfWork1.UserWriter.GetByIdAsync(userId);
        var user2 = await unitOfWork2.UserWriter.GetByIdAsync(userId);

        user1.Should().NotBeNull();
        user2.Should().NotBeNull();

        // Verify both users have the same initial version
        user1.Version.Should().Be(user2.Version);

        // Modify both instances
        user1.UpdatePersonalInfo(FullName.Create("First", "Update").Value, user1.Phone);
        user2.UpdatePersonalInfo(FullName.Create("Second", "Update").Value, user2.Phone);

        // Act & Assert
        // First update should succeed
        await unitOfWork1.UserWriter.UpdateAsync(user1);
        await unitOfWork1.SaveChangesAsync();

        // Second update should fail due to concurrency conflict
        await unitOfWork2.UserWriter.UpdateAsync(user2);

        var act = () => unitOfWork2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
