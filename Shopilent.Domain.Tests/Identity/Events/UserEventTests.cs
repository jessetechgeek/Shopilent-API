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
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);

        // Assert
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserCreatedEvent);
        var createdEvent = (UserCreatedEvent)domainEvent;
        Assert.Equal(user.Id, createdEvent.UserId);
    }

    [Fact]
    public void User_WhenUpdated_ShouldRaiseUserUpdatedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        var newFullNameResult = FullName.Create("John", "Doe");
        Assert.True(newFullNameResult.IsSuccess);

        // Act
        var updateResult = user.UpdatePersonalInfo(newFullNameResult.Value);
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserUpdatedEvent);
        var updatedEvent = (UserUpdatedEvent)domainEvent;
        Assert.Equal(user.Id, updatedEvent.UserId);
    }

    [Fact]
    public void User_WhenEmailChanged_ShouldRaiseUserEmailChangedEvent()
    {
        // Arrange
        var oldEmailResult = Email.Create("old@example.com");
        Assert.True(oldEmailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            oldEmailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event
        
        var newEmailResult = Email.Create("new@example.com");
        Assert.True(newEmailResult.IsSuccess);
        var newEmail = newEmailResult.Value;

        // Act
        var updateResult = user.UpdateEmail(newEmail);
        Assert.True(updateResult.IsSuccess);

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
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "old_password_hash",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        var updateResult = user.UpdatePassword("new_password_hash");
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserPasswordChangedEvent);
        var passwordEvent = (UserPasswordChangedEvent)domainEvent;
        Assert.Equal(user.Id, passwordEvent.UserId);
    }

    [Fact]
    public void User_WhenRoleChanged_ShouldRaiseUserRoleChangedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        var setRoleResult = user.SetRole(UserRole.Manager);
        Assert.True(setRoleResult.IsSuccess);

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
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act - record 5 failures to trigger lockout
        for (int i = 0; i < 4; i++)
        {
            var failResult = user.RecordLoginFailure();
            Assert.True(failResult.IsSuccess);
        }
        var finalFailResult = user.RecordLoginFailure();
        Assert.True(finalFailResult.IsFailure);

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserLockedOutEvent);
        var lockedEvent = (UserLockedOutEvent)domainEvent;
        Assert.Equal(user.Id, lockedEvent.UserId);
    }

    [Fact]
    public void User_WhenActivated_ShouldRaiseUserStatusChangedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        
        var deactivateResult = user.Deactivate(); // Deactivate first
        Assert.True(deactivateResult.IsSuccess);
        
        user.ClearDomainEvents(); // Clear previous events

        // Act
        var activateResult = user.Activate();
        Assert.True(activateResult.IsSuccess);

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
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        var deactivateResult = user.Deactivate();
        Assert.True(deactivateResult.IsSuccess);

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
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);
        
        var fullNameResult = FullName.Create("John", "Doe");
        Assert.True(fullNameResult.IsSuccess);
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        
        var tokenResult = user.GenerateEmailVerificationToken();
        Assert.True(tokenResult.IsSuccess);
        
        user.ClearDomainEvents(); // Clear previous events

        // Act
        var verifyResult = user.VerifyEmail();
        Assert.True(verifyResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(user.DomainEvents, e => e is UserEmailVerifiedEvent);
        var emailVerifiedEvent = (UserEmailVerifiedEvent)domainEvent;
        Assert.Equal(user.Id, emailVerifiedEvent.UserId);
    }
}