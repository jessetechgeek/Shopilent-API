using System;
using Xunit;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.Events;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Identity;

public class UserTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var email = Email.Create("user@example.com");
        var passwordHash = "hashed_password";
        var fullName = new FullName("John", "Doe");

        // Act
        var user = User.Create(email, passwordHash, fullName);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.True(user.IsActive);
        Assert.False(user.EmailVerified);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Empty(user.Addresses);
        Assert.Empty(user.RefreshTokens);
        Assert.Empty(user.Orders);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        Email email = null;
        var passwordHash = "hashed_password";
        var fullName = new FullName("John", "Doe");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => User.Create(email, passwordHash, fullName));
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_ShouldThrowArgumentException()
    {
        // Arrange
        var email = Email.Create("user@example.com");
        var passwordHash = string.Empty;
        var fullName = new FullName("John", "Doe");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => User.Create(email, passwordHash, fullName));
        Assert.Equal("Password hash cannot be empty (Parameter 'passwordHash')", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var email = Email.Create("user@example.com");
        var passwordHash = "hashed_password";
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => User.Create(email, passwordHash, new FullName("", "Doe")));
        Assert.Equal("First name cannot be empty (Parameter 'firstName')", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Arrange
        var email = Email.Create("user@example.com");
        var passwordHash = "hashed_password";
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => User.Create(email, passwordHash, new FullName("John", "")));
        Assert.Equal("Last name cannot be empty (Parameter 'lastName')", exception.Message);
    }

    [Fact]
    public void CreatePreVerified_ShouldCreateVerifiedUser()
    {
        // Arrange
        var email = Email.Create("user@example.com");
        var passwordHash = "hashed_password";
        var fullName = new FullName("John", "Doe");

        // Act
        var user = User.CreatePreVerified(email, passwordHash, fullName);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.True(user.IsActive);
        Assert.True(user.EmailVerified);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void CreateAdmin_ShouldCreateAdminUser()
    {
        // Arrange
        var email = Email.Create("admin@example.com");
        var passwordHash = "hashed_password";
        var fullName = new FullName("Admin", "User");

        // Act
        var user = User.CreateAdmin(email, passwordHash, fullName);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void CreateManager_ShouldCreateManagerUser()
    {
        // Arrange
        var email = Email.Create("manager@example.com");
        var passwordHash = "hashed_password";
        var fullName = new FullName("Manager", "User");

        // Act
        var user = User.CreateManager(email, passwordHash, fullName);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.True(user.IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserCreatedEvent);
    }

    [Fact]
    public void UpdatePersonalInfo_WithValidParameters_ShouldUpdateUserInfo()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        var newFullName = new FullName("Jane", "Smith");
        var newPhone = new PhoneNumber("555-123-4567");

        // Act
        user.UpdatePersonalInfo(newFullName, newPhone);

        // Assert
        Assert.Equal(newFullName, user.FullName);
        Assert.Equal(newPhone, user.Phone);
        Assert.Contains(user.DomainEvents, e => e is UserUpdatedEvent);
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmailAndResetVerification()
    {
        // Arrange
        var user = User.CreatePreVerified(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        Assert.True(user.EmailVerified);
        
        var newEmail = Email.Create("new-email@example.com");

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        Assert.Equal(newEmail, user.Email);
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationExpires);
        Assert.Contains(user.DomainEvents, e => e is UserEmailChangedEvent);
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordAndRevokeTokens()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "old_hashed_password",
            new FullName("John", "Doe"));
        
        var token = "refresh_token";
        var expiry = DateTime.UtcNow.AddDays(7);
        user.AddRefreshToken(token, expiry);
        Assert.Single(user.RefreshTokens);
        Assert.True(user.RefreshTokens.First().IsActive);
        
        var newPasswordHash = "new_hashed_password";

        // Act
        user.UpdatePassword(newPasswordHash);

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.Single(user.RefreshTokens);
        Assert.False(user.RefreshTokens.First().IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserPasswordChangedEvent);
    }

    [Fact]
    public void SetRole_ShouldUpdateUserRole()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        Assert.Equal(UserRole.Customer, user.Role);

        // Act
        user.SetRole(UserRole.Manager);

        // Assert
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.Contains(user.DomainEvents, e => e is UserRoleChangedEvent);
    }

    [Fact]
    public void RecordLoginSuccess_ShouldUpdateLoginInfo()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.RecordLoginFailure(); // Set failed attempt
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.NotNull(user.LastFailedAttempt);

        // Act
        user.RecordLoginSuccess();

        // Assert
        Assert.NotNull(user.LastLogin);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LastFailedAttempt);
    }

    [Fact]
    public void RecordLoginFailure_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        Assert.Equal(0, user.FailedLoginAttempts);

        // Act
        user.RecordLoginFailure();

        // Assert
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.NotNull(user.LastFailedAttempt);
    }

    [Fact]
    public void RecordLoginFailure_ExceedingMaxAttempts_ShouldLockAccount()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        
        // Act - record 5 failed attempts
        for (int i = 0; i < 5; i++)
        {
            user.RecordLoginFailure();
        }

        // Assert
        Assert.Equal(5, user.FailedLoginAttempts);
        Assert.False(user.IsActive); // Account should be locked
        Assert.Contains(user.DomainEvents, e => e is UserLockedOutEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateUser()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.Deactivate();
        Assert.False(user.IsActive);

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LastFailedAttempt);
        Assert.Contains(user.DomainEvents, e => e is UserStatusChangedEvent);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateUser()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        Assert.True(user.IsActive);
        
        var token = "refresh_token";
        var expiry = DateTime.UtcNow.AddDays(7);
        user.AddRefreshToken(token, expiry);
        Assert.Single(user.RefreshTokens);
        Assert.True(user.RefreshTokens.First().IsActive);

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
        Assert.Single(user.RefreshTokens);
        Assert.False(user.RefreshTokens.First().IsActive);
        Assert.Contains(user.DomainEvents, e => e is UserStatusChangedEvent);
    }

    [Fact]
    public void VerifyEmail_ShouldMarkEmailAsVerified()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.GenerateEmailVerificationToken();
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationExpires);

        // Act
        user.VerifyEmail();

        // Assert
        Assert.True(user.EmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationExpires);
        Assert.Contains(user.DomainEvents, e => e is UserEmailVerifiedEvent);
    }

    [Fact]
    public void GenerateEmailVerificationToken_ShouldCreateToken()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationExpires);

        // Act
        user.GenerateEmailVerificationToken();

        // Assert
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationExpires);
        Assert.True(user.EmailVerificationExpires > DateTime.UtcNow);
    }

    [Fact]
    public void GeneratePasswordResetToken_ShouldCreateToken()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetExpires);

        // Act
        user.GeneratePasswordResetToken();

        // Assert
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetExpires);
        Assert.True(user.PasswordResetExpires > DateTime.UtcNow);
    }

    [Fact]
    public void ClearPasswordResetToken_ShouldClearToken()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.GeneratePasswordResetToken();
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetExpires);

        // Act
        user.ClearPasswordResetToken();

        // Assert
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetExpires);
    }

    [Fact]
    public void AddRefreshToken_ShouldAddNewToken()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        var token = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var ipAddress = "127.0.0.1";
        var userAgent = "Test Agent";

        // Act
        var refreshToken = user.AddRefreshToken(token, expiresAt, ipAddress, userAgent);

        // Assert
        Assert.Single(user.RefreshTokens);
        Assert.Equal(token, refreshToken.Token);
        Assert.Equal(expiresAt, refreshToken.ExpiresAt);
        Assert.Equal(ipAddress, refreshToken.IpAddress);
        Assert.Equal(userAgent, refreshToken.UserAgent);
        Assert.True(refreshToken.IsActive);
    }

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeToken()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        var token = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = user.AddRefreshToken(token, expiresAt);
        Assert.True(refreshToken.IsActive);
        var reason = "Test revocation";

        // Act
        user.RevokeRefreshToken(token, reason);

        // Assert
        Assert.False(refreshToken.IsActive);
        Assert.Equal(reason, refreshToken.RevokedReason);
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAllTokens()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        
        var token1 = user.AddRefreshToken("token1", DateTime.UtcNow.AddDays(7));
        var token2 = user.AddRefreshToken("token2", DateTime.UtcNow.AddDays(7));
        var token3 = user.AddRefreshToken("token3", DateTime.UtcNow.AddDays(7));
        
        Assert.Equal(3, user.RefreshTokens.Count);
        Assert.True(token1.IsActive);
        Assert.True(token2.IsActive);
        Assert.True(token3.IsActive);
        
        var reason = "Security measure";

        // Act
        user.RevokeAllRefreshTokens(reason);

        // Assert
        Assert.Equal(3, user.RefreshTokens.Count);
        Assert.All(user.RefreshTokens, token => Assert.False(token.IsActive));
        Assert.All(user.RefreshTokens, token => Assert.Equal(reason, token.RevokedReason));
    }

    [Fact]
    public void AddAddress_ShouldAddNewAddress()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        
        var postalAddress = new PostalAddress(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345"
        );
        
        var addressType = AddressType.Shipping;

        // Act
        var address = user.AddAddress(
            postalAddress,
            addressType);

        // Assert
        Assert.Single(user.Addresses);
        Assert.Equal(postalAddress, address.PostalAddress);
        Assert.Equal(addressType, address.AddressType);
        Assert.False(address.IsDefault);
    }

    [Fact]
    public void AddAddress_WithDefault_ShouldUpdateOtherAddresses()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        
        // Add first default shipping address
        var firstAddress = user.AddAddress(
            new PostalAddress(
                "123 Main St",
                "Anytown",
                "State",
                "Country",
                "12345"
            ),
            AddressType.Shipping,
            null,
            true);
        
        Assert.True(firstAddress.IsDefault);
        
        // Act - add second default shipping address
        var secondAddress = user.AddAddress(
            new PostalAddress(
                "456 Oak Ave",
                "Othertown",
                "State",
                "Country",
                "67890"
            ),
            AddressType.Shipping,
            null,
            true);

        // Assert
        Assert.Equal(2, user.Addresses.Count);
        Assert.False(firstAddress.IsDefault);
        Assert.True(secondAddress.IsDefault);
    }

    [Fact]
    public void RemoveAddress_ShouldRemoveAddress()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        
        var address = user.AddAddress(
            new PostalAddress(
                "123 Main St",
                "Anytown",
                "State",
                "Country",
                "12345"
            ));
        
        Assert.Single(user.Addresses);

        // Act
        user.RemoveAddress(address.Id);

        // Assert
        Assert.Empty(user.Addresses);
    }
}