using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Identity.Read;

[Collection("IntegrationTests")]
public class UserReadRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public UserReadRepositoryTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.UserReader.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email.Value);
        result.FirstName.Should().Be(user.FullName.FirstName);
        result.LastName.Should().Be(user.FullName.LastName);
        result.Role.Should().Be(user.Role);
        result.IsActive.Should().Be(user.IsActive);
        result.EmailVerified.Should().Be(user.EmailVerified);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.UserReader.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDetailByIdAsync_ExistingUser_ShouldReturnUserDetail()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithPhoneNumber("+1234567890")
            .WithVerifiedEmail()
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.UserReader.GetDetailByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email.Value);
        result.FirstName.Should().Be(user.FullName.FirstName);
        result.LastName.Should().Be(user.FullName.LastName);
        result.Phone.Should().Be(user.Phone?.Value);
        result.Role.Should().Be(user.Role);
        result.IsActive.Should().Be(user.IsActive);
        result.EmailVerified.Should().Be(user.EmailVerified);
        result.CreatedAt.Should().BeCloseTo(user.CreatedAt, TimeSpan.FromMilliseconds(100));
        result.UpdatedAt.Should().BeCloseTo(user.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task GetDetailByIdAsync_NonExistentUser_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.UserReader.GetDetailByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("test@example.com")
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.UserReader.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be(user.FullName.FirstName);
        result.LastName.Should().Be(user.FullName.LastName);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _unitOfWork.UserReader.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ShouldReturnUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("Test@Example.com")
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.UserReader.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        // The repository should find the user regardless of case, but may normalize the email
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("existing@example.com")
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var exists = await _unitOfWork.UserReader.EmailExistsAsync("existing@example.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var exists = await _unitOfWork.UserReader.EmailExistsAsync("nonexistent@example.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_WithExcludeId_ShouldExcludeSpecificUser()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random()
            .WithEmail("test@example.com")
            .Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var exists = await _unitOfWork.UserReader.EmailExistsAsync("test@example.com", user.Id);

        // Assert
        exists.Should().BeFalse(); // Should return false because we excluded the user with this email
    }

    [Fact]
    public async Task GetByRoleAsync_AdminRole_ShouldReturnOnlyAdminUsers()
    {
        // Arrange
        await ResetDatabaseAsync();

        var adminUser1 = UserBuilder.AdminUser().Build();
        var adminUser2 = UserBuilder.AdminUser().Build();
        var customerUser = UserBuilder.CustomerUser().Build();

        await _unitOfWork.UserWriter.AddAsync(adminUser1);
        await _unitOfWork.UserWriter.AddAsync(adminUser2);
        await _unitOfWork.UserWriter.AddAsync(customerUser);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var adminUsers = await _unitOfWork.UserReader.GetByRoleAsync("Admin");

        // Assert
        adminUsers.Should().HaveCount(2);
        adminUsers.Should().AllSatisfy(u => u.Role.Should().Be(UserRole.Admin));
        adminUsers.Where(u => u.Id == adminUser1.Id).Should().ContainSingle();
        adminUsers.Where(u => u.Id == adminUser2.Id).Should().ContainSingle();
        adminUsers.Where(u => u.Id == customerUser.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRoleAsync_CustomerRole_ShouldReturnOnlyCustomerUsers()
    {
        // Arrange
        await ResetDatabaseAsync();

        var adminUser = UserBuilder.AdminUser().Build();
        var customerUser1 = UserBuilder.CustomerUser().Build();
        var customerUser2 = UserBuilder.CustomerUser().Build();

        await _unitOfWork.UserWriter.AddAsync(adminUser);
        await _unitOfWork.UserWriter.AddAsync(customerUser1);
        await _unitOfWork.UserWriter.AddAsync(customerUser2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var customerUsers = await _unitOfWork.UserReader.GetByRoleAsync("Customer");

        // Assert
        customerUsers.Should().HaveCount(2);
        customerUsers.Should().AllSatisfy(u => u.Role.Should().Be(UserRole.Customer));
        customerUsers.Where(u => u.Id == customerUser1.Id).Should().ContainSingle();
        customerUsers.Where(u => u.Id == customerUser2.Id).Should().ContainSingle();
        customerUsers.Where(u => u.Id == adminUser.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRoleAsync_NonExistentRole_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CustomerUser().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var users = await _unitOfWork.UserReader.GetByRoleAsync("SuperAdmin");

        // Assert
        users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_ExistingIds_ShouldReturnMatchingUsers()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user1 = UserBuilder.Random().Build();
        var user2 = UserBuilder.Random().Build();
        var user3 = UserBuilder.Random().Build();

        await _unitOfWork.UserWriter.AddAsync(user1);
        await _unitOfWork.UserWriter.AddAsync(user2);
        await _unitOfWork.UserWriter.AddAsync(user3);
        await _unitOfWork.SaveChangesAsync();

        var idsToGet = new[] { user1.Id, user3.Id };

        // Act
        var users = await _unitOfWork.UserReader.GetByIdsAsync(idsToGet);

        // Assert
        users.Should().HaveCount(2);
        users.Where(u => u.Id == user1.Id).Should().ContainSingle();
        users.Where(u => u.Id == user3.Id).Should().ContainSingle();
        users.Where(u => u.Id == user2.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_NonExistentIds_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        var nonExistentIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var users = await _unitOfWork.UserReader.GetByIdsAsync(nonExistentIds);

        // Assert
        users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_EmptyIdsList_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        var emptyIds = Array.Empty<Guid>();

        // Act
        var users = await _unitOfWork.UserReader.GetByIdsAsync(emptyIds);

        // Assert
        users.Should().BeEmpty();
    }


    [Fact]
    public async Task ListAllAsync_MultipleUsers_ShouldReturnAllUsers()
    {
        // Arrange
        await ResetDatabaseAsync();

        var users = UserBuilder.CreateMany(5);
        foreach (var user in users)
        {
            await _unitOfWork.UserWriter.AddAsync(user);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var allUsers = await _unitOfWork.UserReader.ListAllAsync();

        // Assert
        allUsers.Should().HaveCount(5);
        foreach (var user in users)
        {
            allUsers.Where(u => u.Id == user.Id).Should().ContainSingle();
        }
    }

    [Fact]
    public async Task ListAllAsync_NoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var allUsers = await _unitOfWork.UserReader.ListAllAsync();

        // Assert
        allUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_IncludeInactiveUsers_ShouldReturnAllUsers()
    {
        // Arrange
        await ResetDatabaseAsync();

        var activeUser = UserBuilder.Random().AsActive().Build();
        var inactiveUser = UserBuilder.Random().AsInactive().Build();

        await _unitOfWork.UserWriter.AddAsync(activeUser);
        await _unitOfWork.UserWriter.AddAsync(inactiveUser);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var allUsers = await _unitOfWork.UserReader.ListAllAsync();

        // Assert
        allUsers.Should().HaveCount(2);
        allUsers.Where(u => u.Id == activeUser.Id && u.IsActive == true).Should().ContainSingle();
        allUsers.Where(u => u.Id == inactiveUser.Id && u.IsActive == false).Should().ContainSingle();
    }
}
