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
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);

        // Assert
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.DomainEvents.Should().ContainSingle(e => e is UserCreatedEvent);
        var createdEvent = user.DomainEvents.OfType<UserCreatedEvent>().Single();
        createdEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void User_WhenUpdated_ShouldRaiseUserUpdatedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        var newFullNameResult = FullName.Create("John", "Doe");
        newFullNameResult.IsSuccess.Should().BeTrue();

        // Act
        var updateResult = user.UpdatePersonalInfo(newFullNameResult.Value);
        updateResult.IsSuccess.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserUpdatedEvent);
        var updatedEvent = user.DomainEvents.OfType<UserUpdatedEvent>().Single();
        updatedEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void User_WhenEmailChanged_ShouldRaiseUserEmailChangedEvent()
    {
        // Arrange
        var oldEmailResult = Email.Create("old@example.com");
        oldEmailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            oldEmailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event
        
        var newEmailResult = Email.Create("new@example.com");
        newEmailResult.IsSuccess.Should().BeTrue();
        var newEmail = newEmailResult.Value;

        // Act
        var updateResult = user.UpdateEmail(newEmail);
        updateResult.IsSuccess.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEmailChangedEvent);
        var emailEvent = user.DomainEvents.OfType<UserEmailChangedEvent>().Single();
        emailEvent.UserId.Should().Be(user.Id);
        emailEvent.NewEmail.Should().Be(newEmail.Value);
    }

    [Fact]
    public void User_WhenPasswordChanged_ShouldRaiseUserPasswordChangedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "old_password_hash",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        var updateResult = user.UpdatePassword("new_password_hash");
        updateResult.IsSuccess.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserPasswordChangedEvent);
        var passwordEvent = user.DomainEvents.OfType<UserPasswordChangedEvent>().Single();
        passwordEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void User_WhenRoleChanged_ShouldRaiseUserRoleChangedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        var setRoleResult = user.SetRole(UserRole.Manager);
        setRoleResult.IsSuccess.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserRoleChangedEvent);
        var roleEvent = user.DomainEvents.OfType<UserRoleChangedEvent>().Single();
        roleEvent.UserId.Should().Be(user.Id);
        roleEvent.NewRole.Should().Be(UserRole.Manager);
    }

    [Fact]
    public void User_WhenLockedOut_ShouldRaiseUserLockedOutEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act - record 5 failures to trigger lockout
        for (int i = 0; i < 4; i++)
        {
            var failResult = user.RecordLoginFailure();
            failResult.IsSuccess.Should().BeTrue();
        }
        var finalFailResult = user.RecordLoginFailure();
        finalFailResult.IsFailure.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserLockedOutEvent);
        var lockedEvent = user.DomainEvents.OfType<UserLockedOutEvent>().Single();
        lockedEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void User_WhenActivated_ShouldRaiseUserStatusChangedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        
        var deactivateResult = user.Deactivate(); // Deactivate first
        deactivateResult.IsSuccess.Should().BeTrue();
        
        user.ClearDomainEvents(); // Clear previous events

        // Act
        var activateResult = user.Activate();
        activateResult.IsSuccess.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserStatusChangedEvent);
        var statusEvent = user.DomainEvents.OfType<UserStatusChangedEvent>().Single();
        statusEvent.UserId.Should().Be(user.Id);
        statusEvent.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_WhenDeactivated_ShouldRaiseUserStatusChangedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        user.ClearDomainEvents(); // Clear the creation event

        // Act
        var deactivateResult = user.Deactivate();
        deactivateResult.IsSuccess.Should().BeTrue();

        // Assert
        var domainEvent = user.DomainEvents.Should().ContainSingle(e => e is UserStatusChangedEvent).Subject;
        var statusEvent = (UserStatusChangedEvent)domainEvent;
        statusEvent.UserId.Should().Be(user.Id);
        statusEvent.IsActive.Should().BeFalse();
    }

    [Fact]
    public void User_WhenEmailVerified_ShouldRaiseUserEmailVerifiedEvent()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        emailResult.IsSuccess.Should().BeTrue();
        
        var fullNameResult = FullName.Create("John", "Doe");
        fullNameResult.IsSuccess.Should().BeTrue();
        
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        
        var tokenResult = user.GenerateEmailVerificationToken();
        tokenResult.IsSuccess.Should().BeTrue();
        
        user.ClearDomainEvents(); // Clear previous events

        // Act
        var verifyResult = user.VerifyEmail();
        verifyResult.IsSuccess.Should().BeTrue();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEmailVerifiedEvent);
        var emailVerifiedEvent = user.DomainEvents.OfType<UserEmailVerifiedEvent>().Single();
        emailVerifiedEvent.UserId.Should().Be(user.Id);
    }
}