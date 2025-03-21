using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.Events;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.Events;

public class UserEventTests
{
    [Fact]
    public void User_WhenCreated_ShouldRaiseUserCreatedEvent()
    {
        // Act
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserCreatedEvent);
        var createdEvent = (UserCreatedEvent)domainEvent;
        Assert.Equal(user.Id, createdEvent.UserId);
    }

    [Fact]
    public void User_WhenUpdated_ShouldRaiseUserUpdatedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        user.UpdatePersonalInfo(new FullName("John", "Doe"), new PhoneNumber("555-123-4567"));

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserUpdatedEvent);
        var updatedEvent = (UserUpdatedEvent)domainEvent;
        Assert.Equal(user.Id, updatedEvent.UserId);
    }

    [Fact]
    public void User_WhenEmailChanged_ShouldRaiseUserEmailChangedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("old@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.ClearDomainEvents(); // Clear the creation event
        var newEmail = Email.Create("new@example.com");

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserEmailChangedEvent);
        var emailEvent = (UserEmailChangedEvent)domainEvent;
        Assert.Equal(user.Id, emailEvent.UserId);
        Assert.Equal(newEmail.Value, emailEvent.NewEmail);
    }

    [Fact]
    public void User_WhenPasswordChanged_ShouldRaiseUserPasswordChangedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "old_password_hash",
            new FullName("John", "Doe"));
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        user.UpdatePassword("new_password_hash");

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserPasswordChangedEvent);
        var passwordEvent = (UserPasswordChangedEvent)domainEvent;
        Assert.Equal(user.Id, passwordEvent.UserId);
    }

    [Fact]
    public void User_WhenRoleChanged_ShouldRaiseUserRoleChangedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        user.SetRole(UserRole.Manager);

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserRoleChangedEvent);
        var roleEvent = (UserRoleChangedEvent)domainEvent;
        Assert.Equal(user.Id, roleEvent.UserId);
        Assert.Equal(UserRole.Manager, roleEvent.NewRole);
    }

    [Fact]
    public void User_WhenLockedOut_ShouldRaiseUserLockedOutEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.ClearDomainEvents(); // Clear the creation event

        // Act - record 5 failures to trigger lockout
        for (int i = 0; i < 5; i++)
        {
            user.RecordLoginFailure();
        }

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserLockedOutEvent);
        var lockedEvent = (UserLockedOutEvent)domainEvent;
        Assert.Equal(user.Id, lockedEvent.UserId);
    }

    [Fact]
    public void User_WhenActivated_ShouldRaiseUserStatusChangedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.Deactivate(); // Deactivate first
        user.ClearDomainEvents(); // Clear previous events

        // Act
        user.Activate();

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserStatusChangedEvent);
        var statusEvent = (UserStatusChangedEvent)domainEvent;
        Assert.Equal(user.Id, statusEvent.UserId);
        Assert.True(statusEvent.IsActive);
    }

    [Fact]
    public void User_WhenDeactivated_ShouldRaiseUserStatusChangedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        user.Deactivate();

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserStatusChangedEvent);
        var statusEvent = (UserStatusChangedEvent)domainEvent;
        Assert.Equal(user.Id, statusEvent.UserId);
        Assert.False(statusEvent.IsActive);
    }

    [Fact]
    public void User_WhenEmailVerified_ShouldRaiseUserEmailVerifiedEvent()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.GenerateEmailVerificationToken();
        user.ClearDomainEvents(); // Clear previous events

        // Act
        user.VerifyEmail();

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserEmailVerifiedEvent);
        var emailVerifiedEvent = (UserEmailVerifiedEvent)domainEvent;
        Assert.Equal(user.Id, emailVerifiedEvent.UserId);
    }
}